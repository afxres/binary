namespace Mikodev.Binary.Creators.TupleLike
{
    using Mikodev.Binary.Internal;
    using System;
    using System.Collections.Generic;

    internal sealed class KeyValuePairConverter<T1, T2> : Converter<KeyValuePair<T1, T2>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;

        public KeyValuePairConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
        }

        public override void ToBytes(ref Allocator allocator, KeyValuePair<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Key);
            converter2.ToBytes(ref allocator, item.Value);
        }

        public override KeyValuePair<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValue(in temp);
            return new KeyValuePair<T1, T2>(val1, val2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, KeyValuePair<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Key);
            converter2.ToBytesWithMark(ref allocator, item.Value);
        }

        public override KeyValuePair<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            return new KeyValuePair<T1, T2>(val1, val2);
        }
    }

    internal sealed class TupleConverter<T1> : Converter<Tuple<T1>>
    {
        private readonly Converter<T1> converter1;

        public TupleConverter(
            Converter<T1> converter1,
            int length) : base(length)
        {
            this.converter1 = converter1;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytes(ref allocator, item.Item1);
        }

        public override Tuple<T1> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValue(in temp);
            return new Tuple<T1>(val1);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
        }

        public override Tuple<T1> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            return new Tuple<T1>(val1);
        }
    }

    internal sealed class TupleConverter<T1, T2> : Converter<Tuple<T1, T2>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            int length) : base(length)
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

        public override Tuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValue(in temp);
            return new Tuple<T1, T2>(val1, val2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override Tuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            return new Tuple<T1, T2>(val1, val2);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3> : Converter<Tuple<T1, T2, T3>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytes(ref allocator, item.Item3);
        }

        public override Tuple<T1, T2, T3> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValue(in temp);
            return new Tuple<T1, T2, T3>(val1, val2, val3);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
        }

        public override Tuple<T1, T2, T3> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3>(val1, val2, val3);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4> : Converter<Tuple<T1, T2, T3, T4>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytes(ref allocator, item.Item4);
        }

        public override Tuple<T1, T2, T3, T4> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
        }

        public override Tuple<T1, T2, T3, T4> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5> : Converter<Tuple<T1, T2, T3, T4, T5>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytes(ref allocator, item.Item5);
        }

        public override Tuple<T1, T2, T3, T4, T5> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
        }

        public override Tuple<T1, T2, T3, T4, T5> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6> : Converter<Tuple<T1, T2, T3, T4, T5, T6>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytes(ref allocator, item.Item6);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7> : Converter<Tuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;
        private readonly Converter<T7> converter7;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            Converter<T7> converter7,
            int length) : base(length)
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

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValueWithMark(ref temp);
            var val7 = converter7.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
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

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            var val7 = converter7.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7, T8> : Converter<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;
        private readonly Converter<T7> converter7;
        private readonly Converter<T8> converter8;

        public TupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            Converter<T7> converter7,
            Converter<T8> converter8,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
            this.converter7 = converter7;
            this.converter8 = converter8;
        }

        public override void ToBytes(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
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
            converter8.ToBytes(ref allocator, item.Rest);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, T8> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValueWithMark(ref temp);
            var val7 = converter7.ToValueWithMark(ref temp);
            var val8 = converter8.ToValue(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }

        public override void ToBytesWithMark(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
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
            converter8.ToBytesWithMark(ref allocator, item.Rest);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, T8> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            var val7 = converter7.ToValueWithMark(ref span);
            var val8 = converter8.ToValueWithMark(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }
    }

    internal sealed class ValueTupleConverter<T1> : Converter<ValueTuple<T1>>
    {
        private readonly Converter<T1> converter1;

        public ValueTupleConverter(
            Converter<T1> converter1,
            int length) : base(length)
        {
            this.converter1 = converter1;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.ToBytes(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValue(in temp);
            return new ValueTuple<T1>(val1);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            return new ValueTuple<T1>(val1);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2> : Converter<ValueTuple<T1, T2>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytes(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValue(in temp);
            return new ValueTuple<T1, T2>(val1, val2);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2>(val1, val2);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3> : Converter<ValueTuple<T1, T2, T3>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytes(ref allocator, item.Item3);
        }

        public override ValueTuple<T1, T2, T3> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValue(in temp);
            return new ValueTuple<T1, T2, T3>(val1, val2, val3);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
        }

        public override ValueTuple<T1, T2, T3> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3>(val1, val2, val3);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3, T4> : Converter<ValueTuple<T1, T2, T3, T4>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytes(ref allocator, item.Item4);
        }

        public override ValueTuple<T1, T2, T3, T4> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
        }

        public override ValueTuple<T1, T2, T3, T4> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5> : Converter<ValueTuple<T1, T2, T3, T4, T5>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            int length) : base(length)
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

        public override ValueTuple<T1, T2, T3, T4, T5> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
        }

        public override ValueTuple<T1, T2, T3, T4, T5> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5, T6> : Converter<ValueTuple<T1, T2, T3, T4, T5, T6>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytes(ref allocator, item.Item6);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7> : Converter<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;
        private readonly Converter<T7> converter7;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            Converter<T7> converter7,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
            this.converter7 = converter7;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytes(ref allocator, item.Item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValueWithMark(ref temp);
            var val7 = converter7.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytesWithMark(ref allocator, item.Item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            var val7 = converter7.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
        }
    }

    internal sealed class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7, T8> : Converter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>> where T8 : struct
    {
        private readonly Converter<T1> converter1;
        private readonly Converter<T2> converter2;
        private readonly Converter<T3> converter3;
        private readonly Converter<T4> converter4;
        private readonly Converter<T5> converter5;
        private readonly Converter<T6> converter6;
        private readonly Converter<T7> converter7;
        private readonly Converter<T8> converter8;

        public ValueTupleConverter(
            Converter<T1> converter1,
            Converter<T2> converter2,
            Converter<T3> converter3,
            Converter<T4> converter4,
            Converter<T5> converter5,
            Converter<T6> converter6,
            Converter<T7> converter7,
            Converter<T8> converter8,
            int length) : base(length)
        {
            this.converter1 = converter1;
            this.converter2 = converter2;
            this.converter3 = converter3;
            this.converter4 = converter4;
            this.converter5 = converter5;
            this.converter6 = converter6;
            this.converter7 = converter7;
            this.converter8 = converter8;
        }

        public override void ToBytes(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytesWithMark(ref allocator, item.Item7);
            converter8.ToBytes(ref allocator, item.Rest);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.ToValueWithMark(ref temp);
            var val2 = converter2.ToValueWithMark(ref temp);
            var val3 = converter3.ToValueWithMark(ref temp);
            var val4 = converter4.ToValueWithMark(ref temp);
            var val5 = converter5.ToValueWithMark(ref temp);
            var val6 = converter6.ToValueWithMark(ref temp);
            var val7 = converter7.ToValueWithMark(ref temp);
            var val8 = converter8.ToValue(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }

        public override void ToBytesWithMark(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            converter1.ToBytesWithMark(ref allocator, item.Item1);
            converter2.ToBytesWithMark(ref allocator, item.Item2);
            converter3.ToBytesWithMark(ref allocator, item.Item3);
            converter4.ToBytesWithMark(ref allocator, item.Item4);
            converter5.ToBytesWithMark(ref allocator, item.Item5);
            converter6.ToBytesWithMark(ref allocator, item.Item6);
            converter7.ToBytesWithMark(ref allocator, item.Item7);
            converter8.ToBytesWithMark(ref allocator, item.Rest);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.ToValueWithMark(ref span);
            var val2 = converter2.ToValueWithMark(ref span);
            var val3 = converter3.ToValueWithMark(ref span);
            var val4 = converter4.ToValueWithMark(ref span);
            var val5 = converter5.ToValueWithMark(ref span);
            var val6 = converter6.ToValueWithMark(ref span);
            var val7 = converter7.ToValueWithMark(ref span);
            var val8 = converter8.ToValueWithMark(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }
    }
}
