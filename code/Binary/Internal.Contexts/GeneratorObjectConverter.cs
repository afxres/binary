namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal sealed class GeneratorObjectConverter(IGenerator generator) : Converter<object>
{
    private readonly IGenerator generator = generator;

    [DebuggerStepThrough, DoesNotReturn]
    private static void ExceptNull() => throw new ArgumentException("Can not get type of null object.");

    [DebuggerStepThrough, DoesNotReturn]
    private static void ExceptEncode() => throw new NotSupportedException($"Can not encode object, type: {typeof(object)}");

    private IConverter Ensure(object? item)
    {
        if (item is null)
            ExceptNull();
        var type = item.GetType();
        if (type == typeof(object))
            ExceptEncode();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        return this.generator.GetConverter(type);
    }

    private Token Result(byte[]? buffer)
    {
        return new Token(this.generator, new ReadOnlyMemory<byte>(buffer));
    }

    public override void Encode(ref Allocator allocator, object? item) => Ensure(item).Encode(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, object? item) => Ensure(item).EncodeWithLengthPrefix(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, object? item) => Ensure(item).EncodeWithLengthPrefix(ref allocator, item);

    public override byte[] Encode(object? item) => Ensure(item).Encode(item);

    public override object Decode(in ReadOnlySpan<byte> span) => Result(span.ToArray());

    public override object DecodeAuto(ref ReadOnlySpan<byte> span) => Result(Converter.DecodeWithLengthPrefix(ref span).ToArray());

    public override object DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => Result(Converter.DecodeWithLengthPrefix(ref span).ToArray());

    public override object Decode(byte[]? buffer) => Result(buffer);
}
