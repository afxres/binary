namespace Mikodev.Binary.Features;

using Mikodev.Binary.Features.Instance;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;

internal sealed class RawConverterCreator : IConverterCreator
{
    private static readonly ImmutableArray<Type> Types = ImmutableArray.Create(new[]
    {
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(Half),
        typeof(BitVector32),
        typeof(Int128),
        typeof(UInt128),
    });

    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static RawConverterCreator()
    {
        var converters = new IConverter[]
        {
            new RawConverter<DateOnly, DateOnlyRawConverter>(),
            new RawConverter<DateTime, DateTimeRawConverter>(),
            new RawConverter<DateTimeOffset, DateTimeOffsetRawConverter>(),
            new RawConverter<decimal, DecimalRawConverter>(),
            new RawConverter<Guid, GuidRawConverter>(),
            new RawConverter<Rune, RuneRawConverter>(),
            new RawConverter<TimeOnly, TimeOnlyRawConverter>(),
            new RawConverter<TimeSpan, TimeSpanRawConverter>(),
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (Types.Contains(type) is false && type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianRawConverter<>)
                : typeof(LittleEndianRawConverter<>);
            var converterType = typeof(RawConverter<,>).MakeGenericType(type, definition.MakeGenericType(type));
            var converter = CommonModule.CreateInstance(converterType, null);
            return (IConverter)converter;
        }

        if (SharedConverters.TryGetValue(type, out var result))
            return result;
        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
