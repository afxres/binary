using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal ref struct LengthList
    {
        private readonly ReadOnlySpan<byte> span;

        private readonly Span<LengthItem> data;

        private int size;

        public LengthList(ReadOnlySpan<byte> span, Span<LengthItem> data)
        {
            Debug.Assert(span.Length > 0);
            Debug.Assert(data.Length > 0);
            this.span = span;
            this.data = data;
            this.size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Ensure()
        {
            var data = this.data;
            if (size == data.Length)
                return -1;
            for (var i = 0; i < data.Length; i++)
                if (data[i].Offset == 0)
                    return i;
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int cursor, int offset, int length)
        {
            ref var item = ref data[cursor];
            if (item.Offset != 0)
                return false;
            item = new LengthItem(offset, length);
            size++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Invoke<T>(Converter<T> converter, int index)
        {
            var item = data[index];
            var body = span.Slice(item.Offset, item.Length);
            return converter.Decode(in body);
        }
    }
}
