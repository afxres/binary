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
        internal static IConverter GetConverterAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members)
        {
            Debug.Assert(converters.Count == members.Count);
            var encode = GetEncodeDelegateAsTupleObject(type, converters, members, auto: false);
            var decode = GetDecodeDelegateAsTupleObject(type, properties, converters, constructor, indexes, members, auto: false);
            var encodeAuto = GetEncodeDelegateAsTupleObject(type, converters, members, auto: true);
            var decodeAuto = GetDecodeDelegateAsTupleObject(type, properties, converters, constructor, indexes, members, auto: true);
            var itemLength = ContextMethods.GetItemLength(converters);
            var converterArguments = new object[] { encode, decode, encodeAuto, decodeAuto, itemLength };
            var converterType = typeof(TupleObjectConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }

        private static MethodInfo GetEncodeMethodInfo(IConverter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(Allocator).MakeByRefType(), ConverterHelper.GetGenericArgument(converter) };
            var name = auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static MethodInfo GetDecodeMethodInfo(IConverter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var name = auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsTupleObject(Type type, IReadOnlyList<IConverter> converters, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
        {
            Debug.Assert(converters.Count == members.Count);
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < members.Count; i++)
            {
                var converter = converters[i];
                var method = GetEncodeMethodInfo(converter, auto || i != members.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, allocator, members[i].Invoke(item));
                expressions.Add(invoke);
            }
            var delegateType = typeof(OfTupleObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private static Delegate GetDecodeDelegateAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<IConverter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
        {
            IReadOnlyList<Expression> Initialize(ParameterExpression span)
            {
                var values = new Expression[members.Count];
                for (var i = 0; i < members.Count; i++)
                {
                    var converter = converters[i];
                    var method = GetDecodeMethodInfo(converter, auto || i != members.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return values;
            }

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
