using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1, T2> : Converter<Tuple<T1, T2>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2)
            : base(Define.GetConverterLength(converter1, converter2))
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytes(ref allocator, item.Item2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override Tuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = converter1.ToValueWithMark(ref temp);
            var item2 = converter2.ToValue(in temp);
            return new Tuple<T1, T2>(item1, item2);
        }

        public override Tuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = converter1.ToValueWithMark(ref span);
            var item2 = converter2.ToValueWithMark(ref span);
            return new Tuple<T1, T2>(item1, item2);
        }
    }
}
