using Mikodev.Binary.Converters.Runtime;
using Mikodev.Binary.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfTupleObject
    {
        internal static Converter GetConverterAsTupleObject(Type type, ConstructorInfo constructor, ItemIndexes indexes, MetaList metadata)
        {
            var toBytes = ToBytesAsTupleObject(type, metadata, withMark: false);
            var toValue = ToValueAsTupleObject(type, metadata, constructor, indexes, withMark: false);
            var toBytesWith = ToBytesAsTupleObject(type, metadata, withMark: true);
            var toValueWith = ToValueAsTupleObject(type, metadata, constructor, indexes, withMark: true);
            var converterLength = Define.GetConverterLength(metadata.Select(x => x.Converter).ToArray());
            return (Converter)Activator.CreateInstance(typeof(TupleObjectConverter<>).MakeGenericType(type), toBytes, toValue, toBytesWith, toValueWith, converterLength);
        }

        private static Delegate ToBytesAsTupleObject(Type type, MetaList metadata, bool withMark)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var propertyExpression = Expression.Property(item, property);
                var method = Define.GetToBytesMethodInfo(property.PropertyType, withMark || i != metadata.Count - 1);
                expressions.Add(Expression.Call(Expression.Constant(converter), method, allocator, propertyExpression));
            }
            var delegateType = typeof(ToBytesWith<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate ToValueAsTupleObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes, bool withMark)
        {
            if (!ContextMethods.CanCreateInstance(type, constructor))
                return null;
            var delegateType = typeof(ToValueWith<>).MakeGenericType(type);
            return constructor == null
                ? ContextMethods.ToValuePlanAlpha(delegateType, () => InitializeAsTupleObject(metadata, withMark), metadata, type)
                : ContextMethods.ToValuePlanBravo(delegateType, () => InitializeAsTupleObject(metadata, withMark), metadata, indexes, constructor);
        }

        private static (ParameterExpression, Expression[]) InitializeAsTupleObject(MetaList metadata, bool withMark)
        {
            var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
            var values = new Expression[metadata.Count];

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var method = Define.GetToValueMethodInfo(property.PropertyType, withMark || i != metadata.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, span);
                values[i] = invoke;
            }
            return (span, values);
        }
    }
}
