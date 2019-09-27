using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7> : Converter<Tuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        private readonly Converter<T6> converter6;

        private readonly Converter<T7> converter7;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5, Converter<T6> converter6, Converter<T7> converter7)
            : base(Define.GetConverterLength(converter1, converter2, converter3, converter4, converter5, converter6, converter7))
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
            this.converter7 = converter7;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytes(ref allocator, item.Item7);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytesWithMark(ref allocator, item.Item7);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValueWithMark(ref temp);
            var item2 = converter2.ToValueWithMark(ref temp);
            var item3 = converter3.ToValueWithMark(ref temp);
            var item4 = converter4.ToValueWithMark(ref temp);
            var item5 = converter5.ToValueWithMark(ref temp);
            var item6 = converter6.ToValueWithMark(ref temp);
            var item7 = converter7.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            var item2 = converter2.ToValueWithMark(ref span);
            var item3 = converter3.ToValueWithMark(ref span);
            var item4 = converter4.ToValueWithMark(ref span);
            var item5 = converter5.ToValueWithMark(ref span);
            var item6 = converter6.ToValueWithMark(ref span);
            var item7 = converter7.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }
}
