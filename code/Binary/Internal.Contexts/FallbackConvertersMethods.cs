namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Text;

internal static class FallbackConvertersMethods
{
    private static readonly ImmutableDictionary<Type, Type> Types = ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
    {
        [typeof(Uri)] = typeof(UriConverter),
        [typeof(Rune)] = typeof(RuneConverter),
        [typeof(string)] = typeof(StringConverter),
        [typeof(decimal)] = typeof(DecimalConverter),
        [typeof(DateTime)] = typeof(DateTimeConverter),
        [typeof(TimeSpan)] = typeof(TimeSpanConverter),
        [typeof(IPAddress)] = typeof(IPAddressConverter),
        [typeof(IPEndPoint)] = typeof(IPEndPointConverter),
        [typeof(DateTimeOffset)] = typeof(DateTimeOffsetConverter),
    });

    internal static IConverter GetConverter(Type type)
    {
        if (Types.TryGetValue(type, out var converterType) is false)
            return null;
        var converter = Activator.CreateInstance(converterType);
        return (IConverter)converter;
    }
}
