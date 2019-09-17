using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7> : Converter<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Converter<T1> convert1;

        private readonly Converter<T2> convert2;

        private readonly Converter<T3> convert3;

        private readonly Converter<T4> convert4;

        private readonly Converter<T5> convert5;

        private readonly Converter<T6> convert6;

        private readonly Converter<T7> convert7;

        public ValueTupleConverter(Converter<T1> convert1, Converter<T2> convert2, Converter<T3> convert3, Converter<T4> convert4, Converter<T5> convert5, Converter<T6> convert6, Converter<T7> convert7)
            : base(Define.GetConverterLength(convert1, convert2, convert3, convert4, convert5, convert6, convert7))
        {
            this.convert1 = convert1;
            this.convert2 = convert2;
            this.convert3 = convert3;
            this.convert4 = convert4;
            this.convert5 = convert5;
            this.convert6 = convert6;
            this.convert7 = convert7;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytesWithMark(ref allocator, item.Item4);
            convert5.ToBytesWithMark(ref allocator, item.Item5);
            convert6.ToBytesWithMark(ref allocator, item.Item6);
            convert7.ToBytes(ref allocator, item.Item7);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            convert1.ToBytesWithMark(ref allocator, item.Item1);
            convert2.ToBytesWithMark(ref allocator, item.Item2);
            convert3.ToBytesWithMark(ref allocator, item.Item3);
            convert4.ToBytesWithMark(ref allocator, item.Item4);
            convert5.ToBytesWithMark(ref allocator, item.Item5);
            convert6.ToBytesWithMark(ref allocator, item.Item6);
            convert7.ToBytesWithMark(ref allocator, item.Item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item1 = convert1.ToValueWithMark(ref temp);
            var item2 = convert2.ToValueWithMark(ref temp);
            var item3 = convert3.ToValueWithMark(ref temp);
            var item4 = convert4.ToValueWithMark(ref temp);
            var item5 = convert5.ToValueWithMark(ref temp);
            var item6 = convert6.ToValueWithMark(ref temp);
            var item7 = convert7.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item1 = convert1.ToValueWithMark(ref span);
            var item2 = convert2.ToValueWithMark(ref span);
            var item3 = convert3.ToValueWithMark(ref span);
            var item4 = convert4.ToValueWithMark(ref span);
            var item5 = convert5.ToValueWithMark(ref span);
            var item6 = convert6.ToValueWithMark(ref span);
            var item7 = convert7.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }
}
