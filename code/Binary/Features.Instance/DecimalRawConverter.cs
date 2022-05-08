﻿namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct DecimalRawConverter : IRawConverter<decimal>
{
    public static int Length => sizeof(int) * 4;

    public static decimal Decode(ref byte source)
    {
        const int Limits = 4;
        var buffer = (stackalloc int[Limits]);
        for (var i = 0; i < Limits; i++)
            buffer[i] = LittleEndian.Decode<int>(ref Unsafe.Add(ref source, sizeof(int) * i));
        return new decimal(buffer);
    }

    public static void Encode(ref byte target, decimal item)
    {
        const int Limits = 4;
        var buffer = (stackalloc int[Limits]);
        _ = decimal.GetBits(item, buffer);
        for (var i = 0; i < Limits; i++)
            LittleEndian.Encode(ref Unsafe.Add(ref target, sizeof(int) * i), buffer[i]);
        return;
    }
}
#endif
