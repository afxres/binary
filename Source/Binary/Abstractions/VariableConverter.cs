using System;

namespace Mikodev.Binary.Abstractions
{
    /// <summary>
    /// Variable length type converter
    /// </summary>
    public abstract class VariableConverter<T> : Converter<T>
    {
        protected VariableConverter() : base(0) { }

        public override void ToBytesWithMark(ref Allocator allocator, T item) => ToBytesWithLengthPrefix(ref allocator, item);

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span) => ToValueWithLengthPrefix(ref span);
    }
}
