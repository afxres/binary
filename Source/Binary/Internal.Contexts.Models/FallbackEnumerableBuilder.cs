using Mikodev.Binary.Creators.Generics;
using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class FallbackEnumerableBuilder<T> : GenericsBuilder<T, T>
    {
        public override T Invoke(ReadOnlySpan<byte> span, GenericsAdapter<T, T> adapter) => adapter.Decode(span);
    }
}
