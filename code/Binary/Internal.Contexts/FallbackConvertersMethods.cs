namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Converters;
using Mikodev.Binary.Converters.Constants;
using Mikodev.Binary.Converters.Variables;
using System;
using System.Collections.Immutable;

internal static class FallbackConvertersMethods
{
    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static FallbackConvertersMethods()
    {
        var converters = new IConverter[]
        {
            new DateOnlyConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new GuidConverter(),
            new RuneConverter(),
            new TimeOnlyConverter(),
            new TimeSpanConverter(),
            new BigIntegerConverter(),
            new IPAddressConverter(),
            new IPEndPointConverter(),
            new VersionConverter(),
            new StringConverter(),
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    internal static IConverter? GetConverter(Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
