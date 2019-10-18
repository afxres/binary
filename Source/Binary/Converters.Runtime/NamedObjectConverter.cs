using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Runtime
{
    internal sealed class NamedObjectConverter<T> : VariableConverter<T>
    {
        private readonly OfNamedObject<T> ofObject;

        private readonly ToNamedObject<T> toObject;

        private readonly string[] names;

        private readonly HybridList query;

        public NamedObjectConverter(OfNamedObject<T> ofObject, ToNamedObject<T> toObject, KeyValuePair<string, byte[]>[] buffers)
        {
            Debug.Assert(buffers.Any());
            this.ofObject = ofObject;
            this.toObject = toObject;
            names = buffers.Select(x => x.Key).ToArray();
            query = new HybridList(buffers);
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowKeyFound(int i) => throw new ArgumentException($"Property '{names[i]}' already exists, type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowNotFound(int i) => throw new ArgumentException($"Property '{names[i]}' does not exist, type: {ItemType}");

        public override void ToBytes(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            ofObject.Invoke(ref allocator, item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (toObject == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var byteCount = span.Length;
            if (byteCount == 0)
                return default(T) == null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            const int ItemLimits = 16;
            var itemCount = names.Length;
            var items = itemCount > ItemLimits ? new LengthItem[itemCount] : stackalloc LengthItem[itemCount];
            var reader = new LengthReader(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);

            while (reader.Any())
            {
                reader.Update(ref source);
                var index = query.Get(span.Slice(reader.Offset, reader.Length));
                reader.Update(ref source);
                if (index < 0)
                    continue;
                Debug.Assert((uint)index < (uint)itemCount);
                if (items[index].Offset != 0)
                    return ThrowKeyFound(index);
                items[index] = new LengthItem(reader.Offset, reader.Length);
            }

            for (var i = 0; i < itemCount; i++)
                if (items[i].Offset == 0)
                    return ThrowNotFound(i);
            var list = new LengthList(items, in span);
            return toObject.Invoke(in list);
        }
    }
}
