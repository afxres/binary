namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal.Contexts.Decoders;
using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal static class ContextMethodsOfNamedObject
{
    private static readonly MethodInfo AppendMethodInfo = new AllocatorAction<byte[]>(ObjectModule.Append).Method;

    private static readonly MethodInfo InvokeMethodInfo = CommonModule.GetPublicInstanceMethod(typeof(NamedObjectParameter), nameof(NamedObjectParameter.GetValue));

    private static readonly MethodInfo ExistsMethodInfo = CommonModule.GetPublicInstanceMethod(typeof(NamedObjectParameter), nameof(NamedObjectParameter.HasValue));

    private static readonly MethodInfo EnsureMethodInfo = new Func<object, bool>(ObjectModule.NotDefaultValue).Method.GetGenericMethodDefinition();

    private static readonly Expression EnsureSufficientExecutionStackExpression = Expression.Call(new Action(RuntimeHelpers.EnsureSufficientExecutionStack).Method);

    internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor? constructor, ImmutableArray<IConverter> converters, ImmutableArray<bool> optional, ImmutableArray<string> names, ImmutableArray<ImmutableArray<byte>> headers, ImmutableArray<ContextMemberInitializer> members)
    {
        Debug.Assert(members.Length == names.Length);
        Debug.Assert(members.Length == optional.Length);
        Debug.Assert(members.Length == converters.Length);
        var converterType = typeof(NamedObjectDelegateConverter<>).MakeGenericType(type);
        var converter = (IConverter)CommonModule.CreateInstance(converterType, null);
        var hasSelfTypeReference = converters.Any(x => x is IConverterPlaceholder);
        if (hasSelfTypeReference)
            converters = [.. converters.Select(x => x is IConverterPlaceholder ? converter : x)];
        var encode = GetEncodeDelegateAsNamedObject(type, converters, optional, hasSelfTypeReference, headers, members);
        var decode = GetDecodeDelegateAsNamedObject(type, converters, optional, hasSelfTypeReference, constructor);
        var invoke = new NamedObjectDecoder(headers, names, optional, type);
        _ = CommonModule.GetPublicInstanceMethod(converterType, "Initialize").Invoke(converter, [encode, decode, invoke]);
        return converter;
    }

    private static Delegate GetEncodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<bool> optional, bool hasSelfTypeReference, ImmutableArray<ImmutableArray<byte>> headers, ImmutableArray<ContextMemberInitializer> members)
    {
        var item = Expression.Parameter(type, "item");
        var result = new List<Expression>(hasSelfTypeReference ? [EnsureSufficientExecutionStackExpression] : []);
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");

        for (var i = 0; i < members.Length; i++)
        {
            var converter = converters[i];
            var invoke = new List<Expression>();
            var target = members[i].Invoke(item);
            var headed = Allocator.Invoke(headers[i], (ref Allocator allocator, ImmutableArray<byte> header) => Converter.EncodeWithLengthPrefix(ref allocator, header.AsSpan()));
            // append named key with length prefix (cached), then append value with length prefix
            invoke.Add(Expression.Call(AppendMethodInfo, allocator, Expression.Constant(headed)));
            invoke.Add(Expression.Call(Expression.Constant(converter), Converter.GetMethod(converter, nameof(IConverter.EncodeWithLengthPrefix)), allocator, target));
            if (optional[i] is false)
                result.AddRange(invoke);
            else
                result.Add(Expression.IfThen(Expression.Call(EnsureMethodInfo.MakeGenericMethod(target.Type), target), Expression.Block(invoke)));
        }

        var delegateType = typeof(AllocatorAction<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block(result), allocator, item);
        return lambda.Compile();
    }

    private static Delegate? GetDecodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<bool> optional, bool hasSelfTypeReference, ContextObjectConstructor? constructor)
    {
        ContextObjectInitializationData Initialize(ImmutableArray<ParameterExpression> parameters)
        {
            Debug.Assert(parameters.Length is 1);
            var source = parameters[0];
            var result = ImmutableArray.CreateBuilder<Expression>(converters.Length);
            for (var i = 0; i < converters.Length; i++)
            {
                var converter = converters[i];
                var cursor = Expression.Constant(i);
                var method = Converter.GetMethod(converter, nameof(IConverter.Decode));
                var invoke = Expression.Call(source, InvokeMethodInfo, cursor);
                var decode = Expression.Call(Expression.Constant(converter), method, invoke);
                if (optional[i] is false)
                    result.Add(decode);
                else
                    result.Add(Expression.Condition(Expression.Call(source, ExistsMethodInfo, cursor), decode, Expression.Default(method.ReturnType)));
            }
            Debug.Assert(converters.Length == result.Count);
            Debug.Assert(converters.Length == result.Capacity);
            return new ContextObjectInitializationData(hasSelfTypeReference ? [EnsureSufficientExecutionStackExpression] : [], result.MoveToImmutable());
        }

        return constructor?.Invoke(typeof(NamedObjectDecodeDelegate<>).MakeGenericType(type), Initialize);
    }
}
