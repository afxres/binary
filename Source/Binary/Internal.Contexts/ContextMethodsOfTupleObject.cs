﻿using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class ContextMethodsOfTupleObject
    {
        internal static Converter GetConverterAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<Converter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members)
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
            return (Converter)converter;
        }

        private static MethodInfo GetEncodeMethodInfo(Converter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(Allocator).MakeByRefType(), converter.ItemType };
            var name = auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static MethodInfo GetDecodeMethodInfo(Converter converter, bool auto)
        {
            var converterType = converter.GetType();
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var name = auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode);
            var method = converterType.GetMethod(name, types);
            Debug.Assert(method != null);
            return method;
        }

        private static Delegate GetEncodeDelegateAsTupleObject(Type type, IReadOnlyList<Converter> converters, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
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

        private static Delegate GetDecodeDelegateAsTupleObject(Type type, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<Converter> converters, ConstructorInfo constructor, IReadOnlyList<int> indexes, IReadOnlyList<Func<Expression, Expression>> members, bool auto)
        {
            (ParameterExpression, IReadOnlyList<Expression>) Initialize()
            {
                var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
                var values = new Expression[members.Count];

                for (var i = 0; i < members.Count; i++)
                {
                    var converter = converters[i];
                    var method = GetDecodeMethodInfo(converter, auto || i != members.Count - 1);
                    var invoke = Expression.Call(Expression.Constant(converter), method, span);
                    values[i] = invoke;
                }
                return (span, values);
            }

            Debug.Assert(converters.Count == members.Count);
            if (properties != null && !ContextMethods.CanCreateInstance(type, properties, constructor))
                return null;
            var delegateType = typeof(ToTupleObject<>).MakeGenericType(type);
            return constructor is null
                ? ContextMethods.GetDecodeDelegateUseMembers(delegateType, Initialize, members, type)
                : ContextMethods.GetDecodeDelegateUseConstructor(delegateType, Initialize, indexes, constructor);
        }
    }
}
