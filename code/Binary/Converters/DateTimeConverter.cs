﻿using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class DateTimeConverter : Converter<DateTime>
    {
        public DateTimeConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, DateTime item)
        {
            LittleEndian.Encode(ref allocator, item.ToBinary());
        }

        public override DateTime Decode(in ReadOnlySpan<byte> span)
        {
            return DateTime.FromBinary(LittleEndian.Decode<long>(span));
        }
    }
}
