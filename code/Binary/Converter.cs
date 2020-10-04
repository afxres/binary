﻿using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Fallback;
using System;
using System.ComponentModel;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private readonly int length;

        private readonly FallbackAbstractAdapter<T> adapter;

        public int Length => length;

        protected Converter() : this(0) { }

        protected Converter(int length)
        {
            if (length < 0)
                ThrowHelper.ThrowLengthNegative();
            this.length = length;
            this.adapter = length > 0
                ? new FallbackConstantAdapter<T>(this)
                : new FallbackVariableAdapter<T>(this) as FallbackAbstractAdapter<T>;
        }

        public abstract void Encode(ref Allocator allocator, T item);

        public virtual void EncodeAuto(ref Allocator allocator, T item) => adapter.EncodeAuto(ref allocator, item);

        public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item) => adapter.EncodeWithLengthPrefix(ref allocator, item);

        public virtual byte[] Encode(T item) => adapter.Encode(item);

        public abstract T Decode(in ReadOnlySpan<byte> span);

        public virtual T DecodeAuto(ref ReadOnlySpan<byte> span) => adapter.DecodeAuto(ref span);

        public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));

        public virtual T Decode(byte[] buffer) => Decode(new ReadOnlySpan<byte>(buffer));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Converter<T>)}<{typeof(T).Name}>({nameof(Length)}: {Length})";
    }
}