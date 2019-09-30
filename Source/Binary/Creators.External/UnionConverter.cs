using Mikodev.Binary.Delegates;
using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators.External
{
    internal sealed class UnionConverter<T> : Converter<T>
    {
        private const int MarkNone = 0;

        private readonly bool noNull;

        private readonly OfUnion<T> ofUnion;

        private readonly ToUnion<T> toUnion;

        private readonly OfUnion<T> ofUnionWith;

        private readonly ToUnion<T> toUnionWith;

        public UnionConverter(OfUnion<T> ofUnion, ToUnion<T> toUnion, OfUnion<T> ofUnionWith, ToUnion<T> toUnionWith, bool noNull) : base(0)
        {
            this.noNull = noNull;
            this.ofUnion = ofUnion;
            this.toUnion = toUnion;
            this.ofUnionWith = ofUnionWith;
            this.toUnionWith = toUnionWith;
        }

        #region validation & exception
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowNull() => throw new ArgumentNullException("item", $"Union can not be null, type: {ItemType}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowInvalid(int mark) => throw new ArgumentException($"Invalid union tag '{mark}', type: {ItemType}");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowOnNull(T item) { if (noNull && item == null) ThrowNull(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowOnInvalid(int mark) { if (mark != MarkNone) ThrowInvalid(mark); }
        #endregion

        public override void ToBytes(ref Allocator allocator, T item)
        {
            ThrowOnNull(item);
            var mark = MarkNone;
            ofUnion.Invoke(ref allocator, item, ref mark);
            ThrowOnInvalid(mark);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var mark = MarkNone;
            var item = toUnion.Invoke(ref temp, ref mark);
            ThrowOnInvalid(mark);
            return item;
        }

        public override void ToBytesWithMark(ref Allocator allocator, T item)
        {
            ThrowOnNull(item);
            var mark = MarkNone;
            ofUnionWith.Invoke(ref allocator, item, ref mark);
            ThrowOnInvalid(mark);
        }

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var mark = MarkNone;
            var item = toUnionWith.Invoke(ref span, ref mark);
            ThrowOnInvalid(mark);
            return item;
        }
    }
}
