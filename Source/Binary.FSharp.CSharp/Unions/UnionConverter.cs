using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Unions
{
    internal delegate void OfUnion<in T>(ref Allocator allocator, T item, ref int mark);

    internal delegate T ToUnion<out T>(ref ReadOnlySpan<byte> span, ref int mark);

    internal sealed class UnionConverter<T> : Converter<T>
    {
        private const int MarkNone = 0;

        private readonly bool noNull;

        private readonly OfUnion<T> encode;

        private readonly ToUnion<T> decode;

        private readonly OfUnion<T> encodeAuto;

        private readonly ToUnion<T> decodeAuto;

        public UnionConverter(OfUnion<T> encode, ToUnion<T> decode, OfUnion<T> encodeAuto, ToUnion<T> decodeAuto, bool noNull)
        {
            this.noNull = noNull;
            this.encode = encode;
            this.decode = decode;
            this.encodeAuto = encodeAuto;
            this.decodeAuto = decodeAuto;
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowNull() => throw new ArgumentNullException("item", $"Union can not be null, type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowInvalid(int mark) => throw new ArgumentException($"Invalid union tag '{mark}', type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowOnNull(T item) { if (noNull && item == null) ThrowNull(); }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowOnInvalid(int mark) { if (mark != MarkNone) ThrowInvalid(mark); }

        public override void Encode(ref Allocator allocator, T item)
        {
            ThrowOnNull(item);
            var mark = MarkNone;
            encode.Invoke(ref allocator, item, ref mark);
            ThrowOnInvalid(mark);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var mark = MarkNone;
            var item = decode.Invoke(ref temp, ref mark);
            ThrowOnInvalid(mark);
            return item;
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            ThrowOnNull(item);
            var mark = MarkNone;
            encodeAuto.Invoke(ref allocator, item, ref mark);
            ThrowOnInvalid(mark);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var mark = MarkNone;
            var item = decodeAuto.Invoke(ref span, ref mark);
            ThrowOnInvalid(mark);
            return item;
        }
    }
}
