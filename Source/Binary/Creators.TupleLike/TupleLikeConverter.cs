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

        public override void Encode(ref Allocator allocator, KeyValuePair<T1, T2> item)
        {
            converter1.EncodeAuto(ref allocator, item.Key);
            converter2.Encode(ref allocator, item.Value);
        }

        public override KeyValuePair<T1, T2> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.Decode(in temp);
            return new KeyValuePair<T1, T2>(val1, val2);
        }

        public override void EncodeAuto(ref Allocator allocator, KeyValuePair<T1, T2> item)
        {
            converter1.EncodeAuto(ref allocator, item.Key);
            converter2.EncodeAuto(ref allocator, item.Value);
        }

        public override KeyValuePair<T1, T2> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.Encode(ref allocator, item.Item1);
        }

        public override Tuple<T1> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.Decode(in temp);
            return new Tuple<T1>(val1);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
        }

        public override Tuple<T1> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.Encode(ref allocator, item.Item2);
        }

        public override Tuple<T1, T2> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.Decode(in temp);
            return new Tuple<T1, T2>(val1, val2);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
        }

        public override Tuple<T1, T2> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.Encode(ref allocator, item.Item3);
        }

        public override Tuple<T1, T2, T3> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.Decode(in temp);
            return new Tuple<T1, T2, T3>(val1, val2, val3);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
        }

        public override Tuple<T1, T2, T3> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.Encode(ref allocator, item.Item4);
        }

        public override Tuple<T1, T2, T3, T4> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.Decode(in temp);
            return new Tuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3, T4> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
        }

        public override Tuple<T1, T2, T3, T4> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.Encode(ref allocator, item.Item5);
        }

        public override Tuple<T1, T2, T3, T4, T5> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.Decode(in temp);
            return new Tuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
        }

        public override Tuple<T1, T2, T3, T4, T5> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.Encode(ref allocator, item.Item6);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.Decode(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.Encode(ref allocator, item.Item7);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.DecodeAuto(ref temp);
            var val7 = converter7.Decode(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
            var val7 = converter7.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
            converter8.Encode(ref allocator, item.Rest);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, T8> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.DecodeAuto(ref temp);
            var val7 = converter7.DecodeAuto(ref temp);
            var val8 = converter8.Decode(in temp);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
            converter8.EncodeAuto(ref allocator, item.Rest);
        }

        public override Tuple<T1, T2, T3, T4, T5, T6, T7, T8> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
            var val7 = converter7.DecodeAuto(ref span);
            var val8 = converter8.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.Encode(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.Decode(in temp);
            return new ValueTuple<T1>(val1);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
        }

        public override ValueTuple<T1> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.Encode(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.Decode(in temp);
            return new ValueTuple<T1, T2>(val1, val2);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
        }

        public override ValueTuple<T1, T2> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.Encode(ref allocator, item.Item3);
        }

        public override ValueTuple<T1, T2, T3> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.Decode(in temp);
            return new ValueTuple<T1, T2, T3>(val1, val2, val3);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
        }

        public override ValueTuple<T1, T2, T3> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3, T4> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.Encode(ref allocator, item.Item4);
        }

        public override ValueTuple<T1, T2, T3, T4> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.Decode(in temp);
            return new ValueTuple<T1, T2, T3, T4>(val1, val2, val3, val4);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3, T4> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
        }

        public override ValueTuple<T1, T2, T3, T4> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.Encode(ref allocator, item.Item5);
        }

        public override ValueTuple<T1, T2, T3, T4, T5> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.Decode(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5>(val1, val2, val3, val4, val5);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
        }

        public override ValueTuple<T1, T2, T3, T4, T5> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.Encode(ref allocator, item.Item6);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.Decode(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6>(val1, val2, val3, val4, val5, val6);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.Encode(ref allocator, item.Item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.DecodeAuto(ref temp);
            var val7 = converter7.Decode(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(val1, val2, val3, val4, val5, val6, val7);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
            var val7 = converter7.DecodeAuto(ref span);
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

        public override void Encode(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
            converter8.Encode(ref allocator, item.Rest);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var val1 = converter1.DecodeAuto(ref temp);
            var val2 = converter2.DecodeAuto(ref temp);
            var val3 = converter3.DecodeAuto(ref temp);
            var val4 = converter4.DecodeAuto(ref temp);
            var val5 = converter5.DecodeAuto(ref temp);
            var val6 = converter6.DecodeAuto(ref temp);
            var val7 = converter7.DecodeAuto(ref temp);
            var val8 = converter8.Decode(in temp);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }

        public override void EncodeAuto(ref Allocator allocator, ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> item)
        {
            converter1.EncodeAuto(ref allocator, item.Item1);
            converter2.EncodeAuto(ref allocator, item.Item2);
            converter3.EncodeAuto(ref allocator, item.Item3);
            converter4.EncodeAuto(ref allocator, item.Item4);
            converter5.EncodeAuto(ref allocator, item.Item5);
            converter6.EncodeAuto(ref allocator, item.Item6);
            converter7.EncodeAuto(ref allocator, item.Item7);
            converter8.EncodeAuto(ref allocator, item.Rest);
        }

        public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var val1 = converter1.DecodeAuto(ref span);
            var val2 = converter2.DecodeAuto(ref span);
            var val3 = converter3.DecodeAuto(ref span);
            var val4 = converter4.DecodeAuto(ref span);
            var val5 = converter5.DecodeAuto(ref span);
            var val6 = converter6.DecodeAuto(ref span);
            var val7 = converter7.DecodeAuto(ref span);
            var val8 = converter8.DecodeAuto(ref span);
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>(val1, val2, val3, val4, val5, val6, val7, val8);
        }
    }
}
