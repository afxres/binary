using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackSequentialMethods
    {
        private static readonly IReadOnlyDictionary<Type, Type> Types = new Dictionary<Type, Type>
        {
            [typeof(ArraySegment<>)] = typeof(ArraySegmentBuilder<>),
            [typeof(Memory<>)] = typeof(MemoryBuilder<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryBuilder<>),
        };

        private static object CreateArrayBuilder(Type type, Type elementType)
        {
            if (type != elementType.MakeArrayType())
                throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
            return Activator.CreateInstance(typeof(ArrayBuilder<>).MakeGenericType(elementType));
        }

        private static object CreateListBuilder(Type type, Type itemType)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var itemArrayType = itemType.MakeArrayType();
            var arrayField = type.GetField("_items", Flags);
            var countField = type.GetField("_size", Flags);
            if (arrayField is null || arrayField.FieldType != itemArrayType ||
                countField is null || countField.FieldType != typeof(int))
                return null;

            var value = Expression.Parameter(type, "value");
            var array = Expression.Parameter(itemArrayType, "array");
            var count = Expression.Parameter(typeof(int), "count");
            var block = Expression.Block(
                new[] { value },
                Expression.Assign(value, Expression.New(type)),
                Expression.Assign(Expression.Field(value, arrayField), array),
                Expression.Assign(Expression.Field(value, countField), count),
                value);
            var alphaType = typeof(Func<,>).MakeGenericType(type, itemArrayType);
            var bravoType = typeof(Func<,,>).MakeGenericType(itemArrayType, typeof(int), type);
            var alpha = Expression.Lambda(alphaType, Expression.Field(value, arrayField), value);
            var bravo = Expression.Lambda(bravoType, block, array, count);

            var builderArguments = new object[] { alpha.Compile(), bravo.Compile() };
            var builderType = typeof(ListBuilder<>).MakeGenericType(itemType);
            return Activator.CreateInstance(builderType, builderArguments);
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            object Invoke()
            {
                if (type.IsArray && type.GetElementType() is { } elementType)
                    return CreateArrayBuilder(type, elementType);
                if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, x => x == typeof(List<>)))
                    return CreateListBuilder(type, type.GetGenericArguments().Single());
                if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, Types.GetValueOrDefault) is { } result)
                    return Activator.CreateInstance(result.MakeGenericType(type.GetGenericArguments()));
                return null;
            }

            var builder = Invoke();
            if (builder is null)
                return null;
            var itemType = builder.GetType().GetGenericArguments().Single();
            var itemConverter = context.GetConverter(itemType);

            var converterType = typeof(SpanLikeConverter<,>).MakeGenericType(type, itemType);
            var converterArguments = new object[] { builder, itemConverter };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }
    }
}
