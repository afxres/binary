using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class FallbackEnumerableBuilder<T, R> : CollectionBuilder<T, T, R>
    {
        public override T Of(T item) => item;

        public override T To(CollectionAdapter<R> adapter, ReadOnlySpan<byte> span) => (T)(object)adapter.To(span);
    }
}
