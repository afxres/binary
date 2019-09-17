using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1, T2> : Converter<Tuple<T1, T2>>
    {
        private readonly Converter<T1> convert1;

        private readonly Converter<T2> convert2;

        public TupleConverter(Converter<T1> convert1, Converter<T2> convert2)
            : base(Define.GetConverterLength(convert1, convert2))
        {
            this.convert1 = convert1;
            this.convert2 = convert2;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytes(ref allocator, item.Item2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override Tuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValueWithMark(ref temp);
            var item2 = convert2.ToValue(in temp);
            return new Tuple<T1, T2>(item1, item2);
        }

        public override Tuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            var item2 = convert2.ToValueWithMark(ref span);
            return new Tuple<T1, T2>(item1, item2);
        }
    }
}
