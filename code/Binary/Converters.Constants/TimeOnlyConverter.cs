namespace Mikodev.Binary.Converters.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;

internal sealed class TimeOnlyConverter : ConstantConverter<TimeOnly, TimeOnlyConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<TimeOnly>
    {
        public static int Length => sizeof(long);

        public static TimeOnly Decode(ref byte source) => new TimeOnly(LittleEndian.Decode<long>(ref source));

        public static void Encode(ref byte target, TimeOnly item) => LittleEndian.Encode(ref target, item.Ticks);
    }
}
