namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Contexts.Template;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal static class ContextMethodsOfNamedObject
{
    private delegate bool EnsureMethodDelegate<in T>(T? item);

    private delegate void AppendMethodDelegate(ref Allocator allocator, byte[] data);

    private static readonly MethodInfo AppendMethodInfo = new AppendMethodDelegate(NamedObjectTemplates.Append).Method;

    private static readonly MethodInfo InvokeMethodInfo = CommonModule.GetPublicInstanceMethod(typeof(NamedObjectParameter), nameof(NamedObjectParameter.GetValue));

    private static readonly MethodInfo ExistsMethodInfo = CommonModule.GetPublicInstanceMethod(typeof(NamedObjectParameter), nameof(NamedObjectParameter.HasValue));

    private static readonly MethodInfo EnsureMethodInfo = new EnsureMethodDelegate<object>(NamedObjectTemplates.NotDefaultValue).Method.GetGenericMethodDefinition();

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor? constructor, ImmutableArray<IConverter> converters, ImmutableArray<ContextMemberInitializer> members, ImmutableArray<string> names, ImmutableArray<bool> optional, Converter<string> encoding)
    {
        Debug.Assert(members.Length == names.Length);
        Debug.Assert(members.Length == optional.Length);
        Debug.Assert(members.Length == converters.Length);
        var encode = GetEncodeDelegateAsNamedObject(type, converters, members, names, optional, encoding);
        var decode = GetDecodeDelegateAsNamedObject(type, converters, optional, constructor);
        var converterArguments = new object?[] { encoding, names, optional, encode, decode };
        var converterType = typeof(NamedObjectDelegateConverter<>).MakeGenericType(type);
        var converter = CommonModule.CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static Delegate GetEncodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<ContextMemberInitializer> members, ImmutableArray<string> names, ImmutableArray<bool> optional, Converter<string> encoding)
    {
        var item = Expression.Parameter(type, "item");
        var result = new List<Expression>();
        var headers = names.Select(x => Allocator.Invoke(x, encoding.EncodeWithLengthPrefix)).ToList();
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");

        for (var i = 0; i < members.Length; i++)
        {
            var converter = converters[i];
            var invoke = new List<Expression>();
            var target = members[i].Invoke(item);
            // append named key with length prefix (cached), then append value with length prefix
            invoke.Add(Expression.Call(AppendMethodInfo, allocator, Expression.Constant(headers[i])));
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

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static Delegate? GetDecodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<bool> optional, ContextObjectConstructor? constructor)
    {
        ImmutableArray<Expression> Initialize(ImmutableArray<ParameterExpression> parameters)
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
            return result.MoveToImmutable();
        }

        return constructor?.Invoke(typeof(NamedObjectDecodeDelegate<>).MakeGenericType(type), Initialize);
    }
}
