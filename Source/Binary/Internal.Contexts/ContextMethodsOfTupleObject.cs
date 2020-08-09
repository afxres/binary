using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfTupleObject
    {
        internal static IConverter GetConverterAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<Type> types, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members)
        {
            Debug.Assert(converters.Count == types.Count);
            Debug.Assert(converters.Count == members.Count);
            var encode = GetEncodeDelegateAsTupleObject(type, types, converters, members, auto: false);
            var decode = GetDecodeDelegateAsTupleObject(type, properties, types, converters, constructor, indexes, members, auto: false);
            var encodeAuto = GetEncodeDelegateAsTupleObject(type, types, converters, members, auto: true);
            var decodeAuto = GetDecodeDelegateAsTupleObject(type, properties, types, converters, constructor, indexes, members, auto: true);
            var itemLength = ContextMethods.GetItemLength(converters);
            var converterArguments = new object[] { encode, decode, encodeAuto, decodeAuto, itemLength };
            var converterType = typeof(TupleObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static MethodInfo GetEncodeMethodInfo(Type itemType, bool auto)
        {
            var types = new[] { typeof(Allocator).MakeByRefType(), itemType };
            var name = auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode);
            var method = typeof(Converter<>).MakeGenericType(itemType).GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static MethodInfo GetDecodeMethodInfo(Type itemType, bool auto)
        {
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var name = auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode);
            var method = typeof(Converter<>).MakeGenericType(itemType).GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsTupleObject(Type type, IReadOnlyList<Type> types, IReadOnlyList<IConverter> converters, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
        {
            Debug.Assert(converters.Count == types.Count);
            Debug.Assert(converters.Count == members.Count);
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < members.Count; i++)
            {
                var itemType = types[i];
                var converter = converters[i];
                var method = GetEncodeMethodInfo(itemType, auto || i != members.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, allocator, members[i].Invoke(item));
                expressions.Add(invoke);
            }
            var delegateType = typeof(OfTupleObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<Type> types, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
        {
            IReadOnlyList<Expression> Initialize(ParameterExpression span)
            {
                var values = new Expression[members.Count];
                for (var i = 0; i < members.Count; i++)
                {
                    var itemType = types[i];
                    var converter = converters[i];
                    var method = GetDecodeMethodInfo(itemType, auto || i != members.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return values;
            }

            Debug.Assert(converters.Count == types.Count);
            Debug.Assert(converters.Count == members.Count);
            if (properties != null && !ContextMethods.CanCreateInstance(type, properties, constructor))
                return null;
            var delegateType = typeof(ToTupleObject<>).MakeGenericType(type);
            var parameterType = typeof(ReadOnlySpan<byte>).MakeByRefType();
            return constructor is null
                ? ContextMethods.GetDecodeDelegateUseMembers(delegateType, parameterType, Initialize, members, type)
                : ContextMethods.GetDecodeDelegateUseConstructor(delegateType, parameterType, Initialize, indexes, constructor);
        }
    }
}
