using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : Converter
    {
        protected Converter() : this(0) { }

        protected Converter(int length) : base(typeof(T), length) { }

        public abstract void Encode(ref Allocator allocator, T item);

        public abstract T Decode(in ReadOnlySpan<byte> span);
    }
}
