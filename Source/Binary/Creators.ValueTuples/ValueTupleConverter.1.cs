using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverter<T1> : Converter<ValueTuple<T1>>
    {
        private readonly Converter<T1> convert1;

        public ValueTupleConverter(Converter<T1> convert1)
            : base(Define.GetConverterLength(convert1))
        {
            this.convert1 = convert1;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1> item)
        {
            convert1.ToBytes(ref allocator, item.Item1);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1> item)
        {
            convert1.ToBytesWithMark(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValue(in temp);
            return new ValueTuple<T1>(item1);
        }

        public override ValueTuple<T1> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            return new ValueTuple<T1>(item1);
        }
    }
}
