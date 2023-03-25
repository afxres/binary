namespace Mikodev.Binary.Components;

using System;
using System.Collections.Generic;

public static class TupleObject
{
    public static int GetTupleObjectLength(IEnumerable<IConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        var result = 0;
        foreach (var i in converters)
        {
            var length = i.Length;
            if (length is 0)
                return 0;
            checked { result += length; }
        }
        return result;
    }
}
