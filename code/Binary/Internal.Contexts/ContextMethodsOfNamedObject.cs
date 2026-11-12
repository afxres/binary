namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.Contexts.Decoders;
using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal static class ContextMethodsOfNamedObject
{
    private delegate bool ExistsMethodDelegate(ReadOnlySpan<long> data, int index);

    private delegate ReadOnlySpan<byte> InvokeMethodDelegate(ReadOnlySpan<byte> span, ReadOnlySpan<long> data, int index);

    private static readonly MethodInfo AppendMethodInfo = new AllocatorAction<byte[]>(ObjectModule.Append).Method;

    private static readonly MethodInfo InvokeMethodInfo = new InvokeMethodDelegate(GetValueSpan).Method;

    private static readonly MethodInfo ExistsMethodInfo = new ExistsMethodDelegate(HasValue).Method;

    private static readonly MethodInfo EnsureMethodInfo = new Func<object, bool>(ObjectModule.NotDefaultValue).Method.GetGenericMethodDefinition();

    private static readonly Expression EnsureSufficientExecutionStackExpression = Expression.Call(new Action(RuntimeHelpers.EnsureSufficientExecutionStack).Method);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasValue(ReadOnlySpan<long> data, int index)
    {
        return data[index] is not 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<byte> GetValueSpan(ReadOnlySpan<byte> span, ReadOnlySpan<long> data, int index)
    {
        var item = data[index];
        return span.Slice((int)(item >> 32), (int)item);
    }

    internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor? constructor, ImmutableArray<IConverter?> converters, ImmutableArray<bool> optional, ImmutableArray<string> names, ImmutableArray<ContextMemberInitializer> members)
    {
        Debug.Assert(members.Length == names.Length);
        Debug.Assert(members.Length == optional.Length);
        Debug.Assert(members.Length == converters.Length);
        // named keys are always encoded as utf-8, the same as the u8 literals emitted by the source generator
        var headers = names.Select(x => ImmutableArray.Create(Encoding.UTF8.GetBytes(x))).ToImmutableArray();
        var hasSelfTypeReference = converters.Any(x => x is null);
        var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
        var converter = (IConverter)CommonModule.CreateInstance(converterType, null);
        var convertersWithSelf = converters.Select(x => x is null ? converter : x).ToImmutableArray();
        var encode = GetEncodeDelegateAsNamedObject(type, convertersWithSelf, optional, hasSelfTypeReference, headers, members);
        var decode = GetDecodeDelegateAsNamedObject(type, convertersWithSelf, optional, hasSelfTypeReference, constructor);
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
            var headed = Allocator.Invoke(headers[i], (ref allocator, header) => Converter.EncodeWithLengthPrefix(ref allocator, header.AsSpan()));
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
            Debug.Assert(parameters.Length is 2);
            var source = parameters[0];
            var values = parameters[1];
            var result = ImmutableArray.CreateBuilder<Expression>(converters.Length);
            for (var i = 0; i < converters.Length; i++)
            {
                var converter = converters[i];
                var cursor = Expression.Constant(i);
                var method = Converter.GetMethod(converter, nameof(IConverter.Decode));
                var invoke = Expression.Call(InvokeMethodInfo, source, values, cursor);
                var decode = Expression.Call(Expression.Constant(converter), method, invoke);
                if (optional[i] is false)
                    result.Add(decode);
                else
                    result.Add(Expression.Condition(Expression.Call(ExistsMethodInfo, values, cursor), decode, Expression.Default(method.ReturnType)));
            }
            Debug.Assert(converters.Length == result.Count);
            Debug.Assert(converters.Length == result.Capacity);
            return new ContextObjectInitializationData(hasSelfTypeReference ? [EnsureSufficientExecutionStackExpression] : [], result.MoveToImmutable());
        }

        return constructor?.Invoke(typeof(NamedObjectDecodeDelegate<>).MakeGenericType(type), Initialize);
    }
}
