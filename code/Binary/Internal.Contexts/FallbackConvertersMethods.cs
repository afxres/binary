﻿namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Collections.Immutable;

internal static class FallbackConvertersMethods
{
    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static FallbackConvertersMethods()
    {
        var converters = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.Namespace is "Mikodev.Binary.Converters")
            .Select(x => (IConverter)Activator.CreateInstance(x))
            .ToImmutableDictionary(Converter.GetGenericArgument);
        SharedConverters = converters;
    }

    internal static IConverter GetConverter(Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
