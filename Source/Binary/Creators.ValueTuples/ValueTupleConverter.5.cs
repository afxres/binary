using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5> : Converter<ValueTuple<T1, T2, T3, T4, T5>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        public ValueTupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5)
            : base(Define.GetConverterLength(converter1, converter2, converter3, converter4, converter5))
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytes(ref allocator, item.Item5);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
        }

        public override ValueTuple<T1, T2, T3, T4, T5> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValueWithMark(ref temp);
            var item2 = converter2.ToValueWithMark(ref temp);
            var item3 = converter3.ToValueWithMark(ref temp);
            var item4 = converter4.ToValueWithMark(ref temp);
            var item5 = converter5.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public override ValueTuple<T1, T2, T3, T4, T5> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            var item2 = converter2.ToValueWithMark(ref span);
            var item3 = converter3.ToValueWithMark(ref span);
            var item4 = converter4.ToValueWithMark(ref span);
            var item5 = converter5.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }
}
