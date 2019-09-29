using System;

namespace Mikodev.Binary.Creators.TupleLike
{
    internal sealed class ValueTupleConverter<T1, T2> : Converter<ValueTuple<T1, T2>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytes(ref allocator, item.Item2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValueWithMark(ref temp);
            var item2 = converter2.ToValue(in temp);
            return new ValueTuple<T1, T2>(item1, item2);
        }

        public override ValueTuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            var item2 = converter2.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }
}
