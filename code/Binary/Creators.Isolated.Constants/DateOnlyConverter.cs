namespace Mikodev.Binary.Creators.Isolated.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;

internal sealed class DateOnlyConverter : ConstantConverter<DateOnly, DateOnlyConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<DateOnly>
    {
        public static int Length => sizeof(int);

        public static DateOnly Decode(ref byte source) => DateOnly.FromDayNumber(LittleEndian.Decode<int>(ref source));

        public static void Encode(ref byte target, DateOnly item) => LittleEndian.Encode(ref target, item.DayNumber);
    }
}
