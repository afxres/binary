using System;

namespace Mikodev.Binary.Creators.TupleLike
{
    internal sealed class ValueTupleConverter<T1> : Converter<ValueTuple<T1>>
    {
        private readonly Converter<T1> converter1;

        public ValueTupleConverter(
            Converter<T1> converter1,
            int length) : base(length)
        {
            this.converter1 = converter1;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.ToBytes(ref allocator, item.Item1);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValue(in temp);
            return new ValueTuple<T1>(item1);
        }

        public override ValueTuple<T1> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            return new ValueTuple<T1>(item1);
        }
    }
}
