namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

internal static class ContextMethodsOfTupleObject
{
    internal static IConverter GetConverterAsTupleObject(Type type, ContextObjectConstructor? constructor, ImmutableArray<IConverter> converters, ImmutableArray<ContextMemberInitializer> members)
    {
        Debug.Assert(converters.Length == members.Length);
        var encode = GetEncodeDelegateAsTupleObject(type, converters, members, auto: false);
        var encodeAuto = GetEncodeDelegateAsTupleObject(type, converters, members, auto: true);
        var decode = GetDecodeDelegateAsTupleObject(type, converters, constructor, auto: false);
        var decodeAuto = GetDecodeDelegateAsTupleObject(type, converters, constructor, auto: true);
        var itemLength = ContextMethods.GetItemLength(converters);
        var converterArguments = new object?[] { encode, encodeAuto, decode, decodeAuto, itemLength };
        var converterType = typeof(TupleObjectConverter<>).MakeGenericType(type);
        var converter = CommonHelper.CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }

    private static Delegate GetEncodeDelegateAsTupleObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<ContextMemberInitializer> members, bool auto)
    {
        Debug.Assert(converters.Length == members.Length);
        var item = Expression.Parameter(type, "item");
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
        var expressions = new List<Expression>();

        for (var i = 0; i < converters.Length; i++)
        {
            var converter = converters[i];
            var method = Converter.GetMethod(converter, (auto || i != converters.Length - 1) ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode));
            var invoke = Expression.Call(Expression.Constant(converter), method, allocator, members[i].Invoke(item));
            expressions.Add(invoke);
        }
        var delegateType = typeof(EncodeDelegate<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
        return lambda.Compile();
    }

    private static Delegate? GetDecodeDelegateAsTupleObject(Type type, ImmutableArray<IConverter> converters, ContextObjectConstructor? constructor, bool auto)
    {
        ImmutableArray<Expression> Initialize(ImmutableArray<ParameterExpression> parameters)
        {
            Debug.Assert(parameters.Length is 1);
            var source = parameters[0];
            var result = ImmutableArray.CreateBuilder<Expression>(converters.Length);
            for (var i = 0; i < converters.Length; i++)
            {
                var converter = converters[i];
                var method = Converter.GetMethod(converter, (auto || i != converters.Length - 1) ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode));
                var decode = Expression.Call(Expression.Constant(converter), method, source);
                result.Add(decode);
            }
            Debug.Assert(converters.Length == result.Count);
            Debug.Assert(converters.Length == result.Capacity);
            return result.MoveToImmutable();
        }

        return constructor?.Invoke(typeof(DecodeDelegate<>).MakeGenericType(type), Initialize);
    }
}
