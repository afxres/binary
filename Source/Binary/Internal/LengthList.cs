using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal readonly ref struct LengthList
    {
        private readonly ReadOnlySpan<LengthItem> items;

        private readonly ReadOnlySpan<byte> bytes;

        public LengthList(ReadOnlySpan<LengthItem> items, ReadOnlySpan<byte> bytes)
        {
            Debug.Assert(bytes.Length > 0);
            Debug.Assert(items.Length > 0);
            Debug.Assert(items.ToArray().All(x => x.Offset >= sizeof(byte) && x.Length >= 0));
            this.items = items;
            this.bytes = bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>(Converter<T> converter, int index)
        {
            var item = items[index];
            var source = bytes.Slice(item.Offset, item.Length);
            return converter.Decode(in source);
        }
    }
}
