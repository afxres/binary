using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverter<T1, T2> : Converter<ValueTuple<T1, T2>>
    {
        private readonly Converter<T1> convert1;

        private readonly Converter<T2> convert2;

        public ValueTupleConverter(Converter<T1> convert1, Converter<T2> convert2)
            : base(Define.GetConverterLength(convert1, convert2))
        {
            this.convert1 = convert1;
            this.convert2 = convert2;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytes(ref allocator, item.Item2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValueWithMark(ref temp);
            var item2 = convert2.ToValue(in temp);
            return new ValueTuple<T1, T2>(item1, item2);
        }

        public override ValueTuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            var item2 = convert2.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }
}
