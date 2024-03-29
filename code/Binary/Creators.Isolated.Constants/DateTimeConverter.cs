﻿namespace Mikodev.Binary.Creators.Isolated.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;

internal sealed class DateTimeConverter : ConstantConverter<DateTime, DateTimeConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<DateTime>
    {
        public static int Length => sizeof(long);

        public static DateTime Decode(ref byte source) => DateTime.FromBinary(LittleEndian.Decode<long>(ref source));

        public static void Encode(ref byte target, DateTime item) => LittleEndian.Encode(ref target, item.ToBinary());
    }
}
