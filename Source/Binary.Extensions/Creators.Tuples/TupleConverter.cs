using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverter<T1> : TupleLikeConverter<Tuple<T1>>
    {
        private readonly Converter<T1> converter1;

        public TupleConverter(Converter<T1> converter1, int length) : base(length)
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
            var body = span;
            var any1 = converter1.Decode(in body);
            return new Tuple<T1>(any1);
        }

        public override void EncodeAuto(ref Allocator allocator, Tuple<T1> item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            converter1.EncodeAuto(ref allocator, item.Item1);
        }

        public override Tuple<T1> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var any1 = converter1.DecodeAuto(ref span);
            return new Tuple<T1>(any1);
        }
    }

    internal sealed class TupleConverter<T1, T2> : TupleLikeConverter<Tuple<T1, T2>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.Decode(in body);
            return new Tuple<T1, T2>(any1, any2);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            return new Tuple<T1, T2>(any1, any2);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3> : TupleLikeConverter<Tuple<T1, T2, T3>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.Decode(in body);
            return new Tuple<T1, T2, T3>(any1, any2, any3);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3>(any1, any2, any3);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4> : TupleLikeConverter<Tuple<T1, T2, T3, T4>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.DecodeAuto(ref body);
            var any4 = converter4.Decode(in body);
            return new Tuple<T1, T2, T3, T4>(any1, any2, any3, any4);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            var any4 = converter4.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3, T4>(any1, any2, any3, any4);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5> : TupleLikeConverter<Tuple<T1, T2, T3, T4, T5>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.DecodeAuto(ref body);
            var any4 = converter4.DecodeAuto(ref body);
            var any5 = converter5.Decode(in body);
            return new Tuple<T1, T2, T3, T4, T5>(any1, any2, any3, any4, any5);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            var any4 = converter4.DecodeAuto(ref span);
            var any5 = converter5.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3, T4, T5>(any1, any2, any3, any4, any5);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6> : TupleLikeConverter<Tuple<T1, T2, T3, T4, T5, T6>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        private readonly Converter<T6> converter6;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5, Converter<T6> converter6, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.DecodeAuto(ref body);
            var any4 = converter4.DecodeAuto(ref body);
            var any5 = converter5.DecodeAuto(ref body);
            var any6 = converter6.Decode(in body);
            return new Tuple<T1, T2, T3, T4, T5, T6>(any1, any2, any3, any4, any5, any6);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            var any4 = converter4.DecodeAuto(ref span);
            var any5 = converter5.DecodeAuto(ref span);
            var any6 = converter6.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6>(any1, any2, any3, any4, any5, any6);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7> : TupleLikeConverter<Tuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        private readonly Converter<T6> converter6;

        private readonly Converter<T7> converter7;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5, Converter<T6> converter6, Converter<T7> converter7, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.DecodeAuto(ref body);
            var any4 = converter4.DecodeAuto(ref body);
            var any5 = converter5.DecodeAuto(ref body);
            var any6 = converter6.DecodeAuto(ref body);
            var any7 = converter7.Decode(in body);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(any1, any2, any3, any4, any5, any6, any7);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            var any4 = converter4.DecodeAuto(ref span);
            var any5 = converter5.DecodeAuto(ref span);
            var any6 = converter6.DecodeAuto(ref span);
            var any7 = converter7.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(any1, any2, any3, any4, any5, any6, any7);
        }
    }

    internal sealed class TupleConverter<T1, T2, T3, T4, T5, T6, T7, T8> : TupleLikeConverter<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        private readonly Converter<T1> converter1;

        private readonly Converter<T2> converter2;

        private readonly Converter<T3> converter3;

        private readonly Converter<T4> converter4;

        private readonly Converter<T5> converter5;

        private readonly Converter<T6> converter6;

        private readonly Converter<T7> converter7;

        private readonly Converter<T8> converter8;

        public TupleConverter(Converter<T1> converter1, Converter<T2> converter2, Converter<T3> converter3, Converter<T4> converter4, Converter<T5> converter5, Converter<T6> converter6, Converter<T7> converter7, Converter<T8> converter8, int length) : base(length)
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
            var body = span;
            var any1 = converter1.DecodeAuto(ref body);
            var any2 = converter2.DecodeAuto(ref body);
            var any3 = converter3.DecodeAuto(ref body);
            var any4 = converter4.DecodeAuto(ref body);
            var any5 = converter5.DecodeAuto(ref body);
            var any6 = converter6.DecodeAuto(ref body);
            var any7 = converter7.DecodeAuto(ref body);
            var any8 = converter8.Decode(in body);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(any1, any2, any3, any4, any5, any6, any7, any8);
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
            var any1 = converter1.DecodeAuto(ref span);
            var any2 = converter2.DecodeAuto(ref span);
            var any3 = converter3.DecodeAuto(ref span);
            var any4 = converter4.DecodeAuto(ref span);
            var any5 = converter5.DecodeAuto(ref span);
            var any6 = converter6.DecodeAuto(ref span);
            var any7 = converter7.DecodeAuto(ref span);
            var any8 = converter8.DecodeAuto(ref span);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(any1, any2, any3, any4, any5, any6, any7, any8);
        }
    }
}
