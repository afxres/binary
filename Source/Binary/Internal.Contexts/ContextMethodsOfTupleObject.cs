using Mikodev.Binary.Converters.Runtime;
using Mikodev.Binary.Internal.Delegates;
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
            var toBytes = GetToBytesDelegateAsTupleObject(type, metadata, isAuto: false);
            var toValue = GetToValueDelegateAsTupleObject(type, metadata, constructor, indexes, isAuto: false);
            var toBytesWith = GetToBytesDelegateAsTupleObject(type, metadata, isAuto: true);
            var toValueWith = GetToValueDelegateAsTupleObject(type, metadata, constructor, indexes, isAuto: true);
            var converterLength = ContextMethods.GetConverterLength(type, metadata.Select(x => x.Converter).ToArray());
            var converterArguments = new object[] { toBytes, toValue, toBytesWith, toValueWith, converterLength };
            var converter = Activator.CreateInstance(typeof(TupleObjectConverter<>).MakeGenericType(type), converterArguments);
            return (Converter)converter;
        }

        private static Delegate GetToBytesDelegateAsTupleObject(Type type, MetaList metadata, bool isAuto)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var propertyExpression = Expression.Property(item, property);
                var method = ContextMethods.GetToBytesMethodInfo(property.PropertyType, isAuto || i != metadata.Count - 1);
                expressions.Add(Expression.Call(Expression.Constant(converter), method, allocator, propertyExpression));
            }
            var delegateType = typeof(ToBytesWith<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetToValueDelegateAsTupleObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes, bool isAuto)
        {
            (ParameterExpression, Expression[]) Initialize()
            {
                var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
                var values = new Expression[metadata.Count];

                for (var i = 0; i < metadata.Count; i++)
                {
                    var (property, converter) = metadata[i];
                    var method = ContextMethods.GetToValueMethodInfo(property.PropertyType, isAuto || i != metadata.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return (span, values);
            }

            if (!ContextMethods.CanCreateInstance(type, metadata, constructor))
                return null;
            var delegateType = typeof(ToValueWith<>).MakeGenericType(type);
            return constructor == null
                ? ContextMethods.GetToValueDelegateUseProperties(delegateType, Initialize, metadata, type)
                : ContextMethods.GetToValueDelegateUseConstructor(delegateType, Initialize, metadata, indexes, constructor);
        }
    }
}
