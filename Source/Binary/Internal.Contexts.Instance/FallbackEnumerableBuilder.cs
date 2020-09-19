using Mikodev.Binary.Internal.Sequence;
using System;

namespace Mikodev.Binary.Internal.Contexts.Instance
{
    internal sealed class FallbackEnumerableBuilder<T> : SequenceBuilder<T, T>
    {
        public override T Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T, T> adapter) => adapter.Decode(span);
    }
}
