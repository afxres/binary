namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Generic;

public static class TupleObject
{
    public static int GetTupleObjectLength(IEnumerable<IConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        var invoke = false;
        var result = 0;
        foreach (var i in converters)
        {
            invoke = true;
            if (i is not IConverterMetadata)
                throw new ArgumentException($"Sequence contains null or invalid element.", nameof(converters));
            var length = i.Length;
            if (length is 0)
                return 0;
            checked { result += length; }
        }

        if (invoke is false)
            throw new ArgumentException("Sequence contains no element.", nameof(converters));
        return result;
    }
}
