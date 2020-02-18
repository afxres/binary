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
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var arrayField = type.GetField("_items", Flags);
            var countField = type.GetField("_size", Flags);
            if (arrayField is null || arrayField.FieldType != itemType.MakeArrayType() ||
                countField is null || countField.FieldType != typeof(int))
                return null;

            var value = Expression.Parameter(type, "value");
            var array = Expression.Parameter(itemType.MakeArrayType(), "array");
            var count = Expression.Parameter(typeof(int), "count");
            var block = Expression.Block(
                new[] { value },
                Expression.Assign(value, Expression.New(type)),
                Expression.Assign(Expression.Field(value, arrayField), array),
                Expression.Assign(Expression.Field(value, countField), count),
                value);
            var alpha = Expression.Lambda(typeof(OfList<>).MakeGenericType(itemType), Expression.Field(value, arrayField), value);
            var bravo = Expression.Lambda(typeof(ToList<>).MakeGenericType(itemType), block, array, count);

            var itemConverter = context.GetConverter(itemType);
            var builderArguments = new object[] { alpha.Compile(), bravo.Compile() };
            var builderType = typeof(ListBuilder<>).MakeGenericType(itemType);
            var builder = Activator.CreateInstance(builderType, builderArguments);
            var converterArguments = new object[] { itemConverter, builder };
            var converterType = typeof(ArrayLikeAdaptedConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
