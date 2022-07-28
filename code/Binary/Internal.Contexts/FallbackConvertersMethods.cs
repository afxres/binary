namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Converters;
using System;
using System.Collections.Immutable;

internal static class FallbackConvertersMethods
{
    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static FallbackConvertersMethods()
    {
        var converters = new IConverter[]
        {
            new BigIntegerConverter(),
            new BitArrayConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new GuidConverter(),
            new IPAddressConverter(),
            new IPEndPointConverter(),
            new StringConverter(),
            new TimeSpanConverter(),
#if NET5_0_OR_GREATER
            new RuneConverter(),
#endif
#if NET6_0_OR_GREATER
            new DateOnlyConverter(),
            new TimeOnlyConverter(),
#endif
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    internal static IConverter? GetConverter(Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
