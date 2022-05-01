namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct TimeOnlyRawConverter : IRawConverter<TimeOnly>
{
    public static int Length => sizeof(long);

    public static TimeOnly Decode(ref byte source) => new TimeOnly(LittleEndian.Decode<long>(ref source));

    public static void Encode(ref byte target, TimeOnly item) => LittleEndian.Encode(ref target, item.Ticks);
}
#endif
