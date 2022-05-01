namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct DateTimeRawConverter : IRawConverter<DateTime>
{
    public static int Length => sizeof(long);

    public static DateTime Decode(ref byte source) => DateTime.FromBinary(LittleEndian.Decode<long>(ref source));

    public static void Encode(ref byte target, DateTime item) => LittleEndian.Encode(ref target, item.ToBinary());
}
#endif
