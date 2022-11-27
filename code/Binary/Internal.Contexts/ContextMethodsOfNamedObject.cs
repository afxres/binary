namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Components;
using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Contexts.Template;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

internal static class ContextMethodsOfNamedObject
{
    private delegate bool EnsureMethodDelegate<T>(T? item);

    private delegate bool ExistsMethodDelegate(ReadOnlySpan<long> data, int index);

    private delegate void AppendMethodDelegate(ref Allocator allocator, ReadOnlySpan<byte> span);

    private delegate ReadOnlySpan<byte> CreateMethodDelegate(byte[]? data);

    private delegate ReadOnlySpan<byte> InvokeMethodDelegate(ReadOnlySpan<byte> span, ReadOnlySpan<long> data, int index);

    private static readonly MethodInfo AppendMethodInfo = new AppendMethodDelegate(Allocator.Append).Method;

    private static readonly MethodInfo CreateMethodInfo = new CreateMethodDelegate(BridgeModule.CreateReadOnlySpan).Method;

    private static readonly MethodInfo InvokeMethodInfo = new InvokeMethodDelegate(NamedObjectTemplates.GetIndexSpan).Method;

    private static readonly MethodInfo ExistsMethodInfo = new ExistsMethodDelegate(NamedObjectTemplates.HasIndexData).Method;

    private static readonly MethodInfo EnsureMethodInfo = new EnsureMethodDelegate<object>(BridgeModule.NotDefaultValue).Method.GetGenericMethodDefinition();

    internal static IConverter GetConverterAsNamedObject(Type type, ContextObjectConstructor? constructor, ImmutableArray<IConverter> converters, ImmutableArray<PropertyInfo> properties, ImmutableArray<string> names, ImmutableArray<ReadOnlyMemory<byte>> memories, ImmutableArray<bool> required, ByteViewDictionary<int> dictionary)
    {
        Debug.Assert(properties.Length == names.Length);
        Debug.Assert(properties.Length == memories.Length);
        Debug.Assert(properties.Length == required.Length);
        Debug.Assert(properties.Length == converters.Length);
        var encode = GetEncodeDelegateAsNamedObject(type, converters, properties, memories, required);
        var decode = GetDecodeDelegateAsNamedObject(type, converters, required, constructor);
        var converterArguments = new object?[] { encode, decode, names, required, dictionary };
        var converterType = typeof(NamedObjectConverter<>).MakeGenericType(type);
        var converter = CommonModule.CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }

    private static Delegate GetEncodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<PropertyInfo> properties, ImmutableArray<ReadOnlyMemory<byte>> memories, ImmutableArray<bool> required)
    {
        static void Action(ref Allocator allocator, ReadOnlyMemory<byte> memory) => Converter.EncodeWithLengthPrefix(ref allocator, memory.Span);
        static byte[] Handle(ReadOnlyMemory<byte> memory) => Allocator.Invoke(memory, Action);

        var item = Expression.Parameter(type, "item");
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
        var result = new List<Expression>();

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var converter = converters[i];
            var invoke = new List<Expression>();
            var target = Expression.Property(item, property);
            // append named key with length prefix (cached), then append value with length prefix
            invoke.Add(Expression.Call(AppendMethodInfo, allocator, Expression.Call(CreateMethodInfo, Expression.Constant(Handle(memories[i])))));
            invoke.Add(Expression.Call(Expression.Constant(converter), Converter.GetMethod(converter, nameof(IConverter.EncodeWithLengthPrefix)), allocator, target));
            if (required[i])
                result.AddRange(invoke);
            else
                result.Add(Expression.IfThen(Expression.Call(EnsureMethodInfo.MakeGenericMethod(property.PropertyType), target), Expression.Block(invoke)));
        }

        var delegateType = typeof(EncodeDelegate<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block(result), allocator, item);
        return lambda.Compile();
    }

    private static Delegate? GetDecodeDelegateAsNamedObject(Type type, ImmutableArray<IConverter> converters, ImmutableArray<bool> required, ContextObjectConstructor? constructor)
    {
        ImmutableArray<Expression> Initialize(ImmutableArray<ParameterExpression> parameters)
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
                if (required[i])
                    result.Add(decode);
                else
                    result.Add(Expression.Condition(Expression.Call(ExistsMethodInfo, values, cursor), decode, Expression.Default(method.ReturnType)));
            }
            Debug.Assert(converters.Length == result.Count);
            Debug.Assert(converters.Length == result.Capacity);
            return result.MoveToImmutable();
        }

        return constructor?.Invoke(typeof(NamedObjectDecodeDelegate<>).MakeGenericType(type), Initialize);
    }
}
