using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal sealed class NamedObjectConverter<T> : Converter<T>
    {
        private readonly OfNamedObject<T> ofObject;

        private readonly ToNamedObject<T> toObject;

        private readonly string[] names;

        private readonly BinaryNode<int> entry;

        public NamedObjectConverter(OfNamedObject<T> ofObject, ToNamedObject<T> toObject, string[] propertyNames)
        {
            Debug.Assert(propertyNames != null && propertyNames.Any());
            this.ofObject = ofObject;
            this.toObject = toObject;
            names = propertyNames;
            var data = propertyNames.Select((x, i) => (Value: x, Index: i)).ToDictionary(x => x.Value, x => x.Index);
            entry = BinaryNodeHelper.Create(Encoding, data);
        }

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private T ThrowKeyFound(int i) => throw new ArgumentException($"Property '{names[i]}' already exists, type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowNotFound(Span<LengthItem> span)
        {
            Debug.Assert(names.Length > 0);
            Debug.Assert(names.Length == span.Length);
            var item = 0;
            for (var i = 0; i < span.Length; i++)
                if (span[i].Offset == 0)
                    item = i;
            throw new ArgumentException($"Property '{names[item]}' does not exist, type: {ItemType}");
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            ofObject.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (toObject == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var byteCount = span.Length;
            if (byteCount == 0)
                return default(T) == null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            const int ItemLimits = 16;
            var itemCount = names.Length;
            var count = 0;
            var items = itemCount > ItemLimits ? new LengthItem[itemCount] : stackalloc LengthItem[itemCount];
            var reader = new LengthReader(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);

            const int NotFound = -1;
            while (reader.Any())
            {
                reader.Update(ref source);
                var slice = span.Slice(reader.Offset, reader.Length);
                var index = BinaryNodeHelper.GetOrDefault(entry, slice)?.Value ?? NotFound;
                reader.Update(ref source);
                if (index == NotFound)
                    continue;
                Debug.Assert((uint)index < (uint)itemCount);
                ref var value = ref items[index];
                if (value.Offset != 0)
                    return ThrowKeyFound(index);
                value = new LengthItem(reader.Offset, reader.Length);
                count++;
            }

            Debug.Assert(itemCount > 0);
            Debug.Assert(itemCount >= count && count >= 0);
            if (count != itemCount)
                ThrowNotFound(items);
            var list = new LengthList(items, span);
            return toObject.Invoke(list);
        }
    }
}
