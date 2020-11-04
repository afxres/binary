using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Fallback;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private readonly int length;

        private readonly FallbackAdapter<T> adapter;

        public int Length => this.length;

        protected Converter() : this(0) { }

        protected Converter(int length)
        {
            if (length < 0)
                ThrowHelper.ThrowLengthNegative();
            this.length = length;
            this.adapter = FallbackAdapterHelper.Create(this);
        }

        public abstract void Encode(ref Allocator allocator, T item);

        public virtual void EncodeAuto(ref Allocator allocator, T item) => this.adapter.EncodeAuto(ref allocator, item);

        public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item) => this.adapter.EncodeWithLengthPrefix(ref allocator, item);

        public virtual byte[] Encode(T item) => this.adapter.Encode(item);

        public abstract T Decode(in ReadOnlySpan<byte> span);

        public virtual T DecodeAuto(ref ReadOnlySpan<byte> span) => this.adapter.DecodeAuto(ref span);

        public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));

        public virtual T Decode(byte[] buffer) => Decode(new ReadOnlySpan<byte>(buffer));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Converter<T>)}<{typeof(T).Name}>({nameof(Length)}: {Length})";
    }
}
