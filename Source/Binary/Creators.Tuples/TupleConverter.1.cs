using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1> : Converter<Tuple<T1>>
    {
        private readonly Converter<T1> convert1;

        public TupleConverter(Converter<T1> convert1)
            : base(Define.GetConverterLength(convert1))
        {
            this.convert1 = convert1;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytes(ref allocator, item.Item1);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            convert1.ToBytesWithMark(ref allocator, item.Item1);
        }

        public override Tuple<T1> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValue(in temp);
            return new Tuple<T1>(item1);
        }

        public override Tuple<T1> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            return new Tuple<T1>(item1);
        }
    }
}
