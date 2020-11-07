using System;

namespace Mikodev.Binary.Internal.Fallback
{
    internal abstract class FallbackDecoder<T>
    {
        public abstract T DecodeAuto(ref ReadOnlySpan<byte> span);
    }
}
