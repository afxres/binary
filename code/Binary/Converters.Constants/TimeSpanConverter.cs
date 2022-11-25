namespace Mikodev.Binary.Converters.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;

internal sealed class TimeSpanConverter : ConstantConverter<TimeSpan, TimeSpanConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<TimeSpan>
    {
        public static int Length => sizeof(long);

        public static TimeSpan Decode(ref byte source) => new TimeSpan(LittleEndian.Decode<long>(ref source));

        public static void Encode(ref byte target, TimeSpan item) => LittleEndian.Encode(ref target, item.Ticks);
    }
}
