﻿#if NET5_0_OR_GREATER

using Mikodev.Binary.Internal;
using System;
using System.Text;

namespace Mikodev.Binary.Creators
{
    internal sealed class RuneConverter : Converter<Rune>
    {
        public RuneConverter() : base(sizeof(int)) { }

        public override void Encode(ref Allocator allocator, Rune item)
        {
            LittleEndian.Encode(ref allocator, item.Value);
        }

        public override Rune Decode(in ReadOnlySpan<byte> span)
        {
            return new Rune(LittleEndian.Decode<int>(span));
        }
    }
}

#endif
