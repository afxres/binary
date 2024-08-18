namespace Mikodev.Binary.Internal;

using System;

internal static class EnsureModule
{
    internal static IConverter GetConverter(IConverter? converter, Type type, Type? creator)
    {
        if (converter is not null && Converter.GetGenericArgument(converter) == type)
            return converter;
        var actual = converter is null ? "null" : converter.GetType().ToString();
        var result = $"Invalid converter, expected: converter for '{type}', actual: {actual}";
        if (creator is not null)
            result = $"{result}, converter creator type: {creator}";
        throw new InvalidOperationException(result);
    }
}
