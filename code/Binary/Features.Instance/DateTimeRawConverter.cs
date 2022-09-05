﻿namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal readonly struct DateTimeRawConverter : IRawConverter<DateTime>
{
    public static int Length => sizeof(long);

    public static DateTime Decode(ref byte source) => DateTime.FromBinary(LittleEndian.Decode<long>(ref source));

    public static void Encode(ref byte target, DateTime item) => LittleEndian.Encode(ref target, item.ToBinary());
}
