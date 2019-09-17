using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1, T2, T3, T4> : Converter<Tuple<T1, T2, T3, T4>>
    {
        private readonly Converter<T1> convert1;

        private readonly Converter<T2> convert2;

        private readonly Converter<T3> convert3;

        private readonly Converter<T4> convert4;

        public TupleConverter(Converter<T1> convert1, Converter<T2> convert2, Converter<T3> convert3, Converter<T4> convert4)
            : base(Define.GetConverterLength(convert1, convert2, convert3, convert4))
        {
            this.convert1 = convert1;
            this.convert2 = convert2;
            this.convert3 = convert3;
            this.convert4 = convert4;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytes(ref allocator, item.Item4);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytesWithMark(ref allocator, item.Item4);
        }

        public override Tuple<T1, T2, T3, T4> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValueWithMark(ref temp);
            var item2 = convert2.ToValueWithMark(ref temp);
            var item3 = convert3.ToValueWithMark(ref temp);
            var item4 = convert4.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public override Tuple<T1, T2, T3, T4> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            var item2 = convert2.ToValueWithMark(ref span);
            var item3 = convert3.ToValueWithMark(ref span);
            var item4 = convert4.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
}
