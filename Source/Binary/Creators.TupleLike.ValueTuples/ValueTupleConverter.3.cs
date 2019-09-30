using System;

namespace Mikodev.Binary.Creators.TupleLike.ValueTuples
{
    internal sealed class ValueTupleConverter<T1, T2, T3> : Converter<ValueTuple<T1, T2, T3>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        public ValueTupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytes(ref allocator, item.Item3);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
        }

        public override ValueTuple<T1, T2, T3> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValueWithMark(ref temp);
            var item2 = converter2.ToValueWithMark(ref temp);
            var item3 = converter3.ToValue(in temp);
            return new ValueTuple<T1, T2, T3>(item1, item2, item3);
        }

        public override ValueTuple<T1, T2, T3> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            var item2 = converter2.ToValueWithMark(ref span);
            var item3 = converter3.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3>(item1, item2, item3);
        }
    }
}
