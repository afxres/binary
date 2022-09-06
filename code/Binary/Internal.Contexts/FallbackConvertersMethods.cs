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
            new IPAddressConverter(),
            new IPEndPointConverter(),
            new StringConverter(),
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    internal static IConverter? GetConverter(Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
