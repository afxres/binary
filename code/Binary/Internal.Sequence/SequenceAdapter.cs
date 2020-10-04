﻿using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal abstract class SequenceAdapter<T, R>
    {
        public abstract void Encode(ref Allocator allocator, T item);

        public abstract R Decode(ReadOnlySpan<byte> span);
    }
}