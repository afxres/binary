namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal readonly struct TimeSpanRawConverter : IRawConverter<TimeSpan>
{
    public static int Length => sizeof(long);

    public static TimeSpan Decode(ref byte source) => new TimeSpan(LittleEndian.Decode<long>(ref source));

    public static void Encode(ref byte target, TimeSpan item) => LittleEndian.Encode(ref target, item.Ticks);
}
