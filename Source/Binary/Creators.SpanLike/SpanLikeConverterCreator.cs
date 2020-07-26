using Mikodev.Binary.Creators.SpanLike.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Creators.SpanLike
{
    internal sealed class SpanLikeConverterCreator : IConverterCreator
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
            var itemArrayType = itemType.MakeArrayType();
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
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

        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            object GetBuilder()
            {
                if (type.IsArray && type.GetElementType() is { } elementType)
                    return CreateArrayBuilder(type, elementType);
                if (type.IsGenericType is false)
                    return null;
                var definition = type.GetGenericTypeDefinition();
                var itemType = type.GetGenericArguments().FirstOrDefault();
                if (definition == typeof(List<>))
                    return CreateListBuilder(type, itemType);
                if (Types.TryGetValue(definition, out var result))
                    return Activator.CreateInstance(result.MakeGenericType(itemType));
                return null;
            }

            var builder = GetBuilder();
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
