using Mikodev.Binary.Converters.Runtime;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfNamedObject
    {
        private static readonly MethodInfo invokeMethodInfo = typeof(LengthList).GetMethod(nameof(LengthList.Invoke), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo appendWithLengthPrefixMethodInfo = typeof(Allocator).GetMethod(nameof(Allocator.AppendWithLengthPrefix), BindingFlags.Instance | BindingFlags.NonPublic);

        internal static Converter GetConverterAsNamedObject(Type type, ConstructorInfo constructor, ItemIndexes indexes, MetaList metadata, NameDictionary dictionary, Func<string, byte[]> cache)
        {
            if (dictionary == null)
                dictionary = metadata.Select(x => x.Property).ToDictionary(x => x, x => x.Name);
            var toBytes = ToBytesAsNamedObject(type, metadata, dictionary, cache);
            var toValue = ToValueAsNamedObject(type, metadata, constructor, indexes);
            var buffers = metadata.Select(x => dictionary[x.Property]).Select(x => new KeyValuePair<string, byte[]>(x, cache.Invoke(x))).ToArray();
            var properties = metadata.Select(x => x.Property).ToArray();
            return (Converter)Activator.CreateInstance(typeof(NamedObjectConverter<>).MakeGenericType(type), toBytes, toValue, properties, buffers);
        }

        private static Delegate ToBytesAsNamedObject(Type type, MetaList metadata, NameDictionary dictionary, Func<string, byte[]> cache)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var buffer = cache.Invoke(dictionary[property]);
                var propertyType = property.PropertyType;
                var propertyExpression = Expression.Property(item, property);
                var methodInfo = typeof(Converter<>).MakeGenericType(propertyType).GetMethod(nameof(IConverter.ToBytesWithLengthPrefix));
                expressions.Add(Expression.Call(allocator, appendWithLengthPrefixMethodInfo, Expression.Constant(buffer)));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, propertyExpression));
            }
            var delegateType = typeof(OfNamedObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate ToValueAsNamedObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes)
        {
            if (!ContextMethods.CanCreateInstance(type, constructor))
                return null;
            var delegateType = typeof(ToNamedObject<>).MakeGenericType(type);
            return constructor == null
                ? ContextMethods.ToValuePlanAlpha(delegateType, () => InitializeAsNamedObject(metadata), metadata, type)
                : ContextMethods.ToValuePlanBravo(delegateType, () => InitializeAsNamedObject(metadata), metadata, indexes, constructor);
        }

        private static (ParameterExpression, Expression[]) InitializeAsNamedObject(MetaList metadata)
        {
            var list = Expression.Parameter(typeof(LengthList).MakeByRefType(), "list");
            var values = new Expression[metadata.Count];

            for (var i = 0; i < metadata.Count; i++)
            {
                var (_, converter) = metadata[i];
                var method = invokeMethodInfo.MakeGenericMethod(converter.ItemType);
                var invoke = Expression.Call(list, method, Expression.Constant(converter), Expression.Constant(i));
                values[i] = invoke;
            }
            return (list, values);
        }
    }
}
