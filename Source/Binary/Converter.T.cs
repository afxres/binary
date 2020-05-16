using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : Converter
    {
        private readonly AbstractAdapter<T> adapter;

        protected Converter() : this(0) { }

        protected Converter(int length) : base(typeof(T), length) => this.adapter = length > 0 ? new ConstantAdapter<T>(this) : new VariableAdapter<T>(this) as AbstractAdapter<T>;

        public abstract void Encode(ref Allocator allocator, T item);

        public abstract T Decode(in ReadOnlySpan<byte> span);

        public virtual void EncodeAuto(ref Allocator allocator, T item) => adapter.EncodeAuto(ref allocator, item);

        public virtual T DecodeAuto(ref ReadOnlySpan<byte> span) => adapter.DecodeAuto(ref span);

        public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item) => adapter.EncodeWithLengthPrefix(ref allocator, item);

        public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));

        public virtual byte[] Encode(T item) => adapter.Encode(item);

        public virtual T Decode(byte[] buffer) => Decode(new ReadOnlySpan<byte>(buffer));
    }
}
