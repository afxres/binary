namespace Mikodev.Binary.WireTests;

using Mikodev.Binary;
using System;
using System.Collections.Generic;
using System.Linq;

public static class WireEncoder
{
    public static IGenerator Reflection { get; } = Generator.CreateDefault();

    // No runtime fallback: a type without a generated converter throws instead of falling back to reflection.
    public static IGenerator Aot { get; } = Generator.CreateAotBuilder()
        .AddConverterCreators(WireGeneratorContext.ConverterCreators.Values)
        .Build();

    public static IReadOnlyDictionary<string, WireCase> Cases { get; } = WireCorpus.All.ToDictionary(x => x.Name, StringComparer.Ordinal);

    public static byte[] Encode(IGenerator generator, WireCase item, bool auto)
    {
        return EncodeValue(generator, item.Type, item.Value, auto);
    }

    public static byte[] EncodeValue(IGenerator generator, Type type, object? value, bool auto)
    {
        var converter = generator.GetConverter(type);
        var allocator = new Allocator(Span<byte>.Empty);
        if (auto)
            converter.EncodeAuto(ref allocator, value);
        else
            converter.Encode(ref allocator, value);
        return allocator.ToArray();
    }

    public static DecodeResult Decode(IGenerator generator, WireCase item, ReadOnlySpan<byte> bytes, bool auto)
    {
        var converter = generator.GetConverter(item.Type);
        var span = bytes;
        // `Decode` takes the span by value and consumes it whole; `DecodeAuto` advances it past the header.
        var value = auto ? converter.DecodeAuto(ref span) : converter.Decode(span);
        return new DecodeResult(value, auto ? bytes.Length - span.Length : bytes.Length);
    }
}

public readonly record struct DecodeResult(object? Value, int Consumed);
