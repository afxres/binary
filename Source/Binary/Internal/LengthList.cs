using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal readonly ref struct LengthList
    {
        private readonly ReadOnlySpan<LengthItem> info;

        private readonly ReadOnlySpan<byte> data;

        public LengthList(ReadOnlySpan<LengthItem> info, ReadOnlySpan<byte> data)
        {
            Debug.Assert(data.Length > 0);
            Debug.Assert(info.Length > 0);
            Debug.Assert(info.ToArray().All(x => x.Offset >= sizeof(byte) && x.Length >= 0));
            this.info = info;
            this.data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>(Converter<T> converter, int index)
        {
            var item = info[index];
            var source = data.Slice(item.Offset, item.Length);
            return converter.Decode(in source);
        }
    }
}
