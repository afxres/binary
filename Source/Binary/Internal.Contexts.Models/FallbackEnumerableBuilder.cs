﻿using Mikodev.Binary.Creators.Sequence;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class FallbackEnumerableBuilder<T> : SequenceBuilder<T, T>
    {
        public override T Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T, T> adapter) => adapter.Decode(span);
    }
}
