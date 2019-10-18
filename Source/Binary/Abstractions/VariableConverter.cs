using System;

namespace Mikodev.Binary.Abstractions
{
    /// <summary>
    /// Variable length type converter
    /// </summary>
    public abstract class VariableConverter<T> : Converter<T>
    {
        protected VariableConverter() : base(0) { }

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefix(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefix(ref span);
    }
}
