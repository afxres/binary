using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7, TR> : Converter<Tuple<T1, T2, T3, T4, T5, T6, T7, TR>>
    {
        private readonly Converter<T1> convert1;

        private readonly Converter<T2> convert2;

        private readonly Converter<T3> convert3;

        private readonly Converter<T4> convert4;

        private readonly Converter<T5> convert5;

        private readonly Converter<T6> convert6;

        private readonly Converter<T7> convert7;

        private readonly Converter<TR> convertR;

        public TupleConverter(Converter<T1> convert1, Converter<T2> convert2, Converter<T3> convert3, Converter<T4> convert4, Converter<T5> convert5, Converter<T6> convert6, Converter<T7> convert7, Converter<TR> convertR)
            : base(Define.GetConverterLength(convert1, convert2, convert3, convert4, convert5, convert6, convert7, convertR))
        {
            this.convert1 = convert1;
            this.convert2 = convert2;
            this.convert3 = convert3;
            this.convert4 = convert4;
            this.convert5 = convert5;
            this.convert6 = convert6;
            this.convert7 = convert7;
            this.convertR = convertR;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, TR> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytesWithMark(ref allocator, item.Item4);
            convert5.ToBytesWithMark(ref allocator, item.Item5);
            convert6.ToBytesWithMark(ref allocator, item.Item6);
            convert7.ToBytesWithMark(ref allocator, item.Item7);
            convertR.ToBytes(ref allocator, item.Rest);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, TR> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytesWithMark(ref allocator, item.Item4);
            convert5.ToBytesWithMark(ref allocator, item.Item5);
            convert6.ToBytesWithMark(ref allocator, item.Item6);
            convert7.ToBytesWithMark(ref allocator, item.Item7);
            convertR.ToBytesWithMark(ref allocator, item.Rest);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, TR> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValueWithMark(ref temp);
            var item2 = convert2.ToValueWithMark(ref temp);
            var item3 = convert3.ToValueWithMark(ref temp);
            var item4 = convert4.ToValueWithMark(ref temp);
            var item5 = convert5.ToValueWithMark(ref temp);
            var item6 = convert6.ToValueWithMark(ref temp);
            var item7 = convert7.ToValueWithMark(ref temp);
            var itemR = convertR.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, TR>(item1, item2, item3, item4, item5, item6, item7, itemR);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, TR> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            var item2 = convert2.ToValueWithMark(ref span);
            var item3 = convert3.ToValueWithMark(ref span);
            var item4 = convert4.ToValueWithMark(ref span);
            var item5 = convert5.ToValueWithMark(ref span);
            var item6 = convert6.ToValueWithMark(ref span);
            var item7 = convert7.ToValueWithMark(ref span);
            var itemR = convertR.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, TR>(item1, item2, item3, item4, item5, item6, item7, itemR);
        }
    }
}
