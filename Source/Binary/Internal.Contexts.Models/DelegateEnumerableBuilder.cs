﻿using Mikodev.Binary.Creators.Sequence;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class DelegateEnumerableBuilder<T, R> : SequenceBuilder<T, R>
    {
        private readonly Func<R, T> constructor;

        public DelegateEnumerableBuilder(Func<R, T> constructor) => this.constructor = constructor;

        public override T Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T, R> adapter)
        {
            if (constructor is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var data = adapter.Decode(span);
            return constructor.Invoke(data);
        }
    }
}
