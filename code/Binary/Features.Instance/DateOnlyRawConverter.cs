namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct DateOnlyRawConverter : IRawConverter<DateOnly>
{
    public static int Length => sizeof(int);

    public static DateOnly Decode(ref byte source) => DateOnly.FromDayNumber(LittleEndian.Decode<int>(ref source));

    public static void Encode(ref byte target, DateOnly item) => LittleEndian.Encode(ref target, item.DayNumber);
}
#endif
