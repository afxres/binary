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
            static void DecodePrefix(ref byte location, ref int offset, ref int length, int limits)
            {
                Debug.Assert((uint)(limits - offset) > (uint)length);
                offset += length;
                ref var source = ref Unsafe.Add(ref location, offset);
                var numberLength = PrimitiveHelper.DecodeNumberLength(source);
                if ((uint)(limits - offset) < (uint)numberLength)
                    goto fail;
                length = PrimitiveHelper.DecodeNumber(ref source, numberLength);
                offset += numberLength;
                if ((uint)(limits - offset) < (uint)length)
                    goto fail;
                return;

            fail:
                ThrowHelper.ThrowNotEnoughBytes();
            }

            if (toObject == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var byteCount = span.Length;
            if (byteCount == 0)
                return default(T) == null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            const int ItemLimits = 16;
            var itemCount = names.Length;
            var count = 0;
            var items = itemCount > ItemLimits ? new LengthItem[itemCount] : stackalloc LengthItem[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);

            const int NotFound = -1;
            var limits = byteCount;
            var offset = 0;
            var length = 0;
            while (limits - offset != length)
            {
                DecodePrefix(ref source, ref offset, ref length, limits);
                var index = BinaryNodeHelper.GetOrDefault(entry, ref Unsafe.Add(ref source, offset), length)?.Value ?? NotFound;
                DecodePrefix(ref source, ref offset, ref length, limits);
                if (index == NotFound)
                    continue;
                Debug.Assert((uint)index < (uint)itemCount);
                ref var value = ref items[index];
                if (value.Offset != 0)
                    return ThrowKeyFound(index);
                value = new LengthItem(offset, length);
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
