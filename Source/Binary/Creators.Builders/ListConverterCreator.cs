using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ListConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetGenericArguments(typeof(List<>), out var types))
                return null;
            var itemType = types.Single();
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

            var itemConverter = context.GetConverter(itemType);
            var builderArguments = new object[] { alpha.Compile(), bravo.Compile() };
            var builderType = typeof(ListBuilder<>).MakeGenericType(itemType);
            var builder = Activator.CreateInstance(builderType, builderArguments);
            var converterArguments = new object[] { builder, itemConverter };
            var converterType = typeof(ArrayLikeAdaptedConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
