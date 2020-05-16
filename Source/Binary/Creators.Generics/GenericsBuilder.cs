using System;

namespace Mikodev.Binary.Creators.Generics
{
    internal abstract class GenericsBuilder<T, R>
    {
        public abstract T Invoke(ReadOnlySpan<byte> span, GenericsAdapter<T, R> adapter);
    }
}
