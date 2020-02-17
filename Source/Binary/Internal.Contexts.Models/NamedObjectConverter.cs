using System;
using System.Collections.Generic;
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

        private readonly ReadOnlyMemory<string> names;

        private readonly BinaryNode<int> entry;

        public NamedObjectConverter(OfNamedObject<T> ofObject, ToNamedObject<T> toObject, BinaryNode<int> entry, IReadOnlyCollection<string> names)
        {
            Debug.Assert(entry != null);
            Debug.Assert(names != null && names.Any());
            this.ofObject = ofObject;
            this.toObject = toObject;
            this.entry = entry;
            this.names = new ReadOnlyMemory<string>(names.ToArray());
        }

        [DebuggerStepThrough]
        private T ThrowKeyFound(int i) => throw new ArgumentException($"Named key '{names.Span[i]}' already exists, type: {ItemType}");

        [DebuggerStepThrough]
        private T ThrowNotFound(int i) => throw new ArgumentException($"Named key '{names.Span[i]}' does not exist, type: {ItemType}");

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
                Debug.Assert((uint)(limits - offset) >= (uint)length);
                offset += length;
                if (limits == offset)
                    goto fail;
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
            var byteLength = span.Length;
            if (byteLength == 0)
                return default(T) == null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            const int ItemLimits = 32;
            var capacity = names.Length;
            var data = capacity > ItemLimits ? new LengthItem[capacity] : stackalloc LengthItem[capacity];
            var list = new LengthList(span, data);
            ref var source = ref MemoryMarshal.GetReference(span);

            var limits = byteLength;
            var offset = 0;
            var length = 0;
            while (limits - offset != length)
            {
                DecodePrefix(ref source, ref offset, ref length, limits);
                var result = BinaryNodeHelper.GetOrDefault(entry, ref Unsafe.Add(ref source, offset), length);
                DecodePrefix(ref source, ref offset, ref length, limits);
                if (result is null || list.Insert(result.Value, offset, length))
                    continue;
                return ThrowKeyFound(result.Value);
            }

            var cursor = list.Ensure();
            if (cursor != -1)
                return ThrowNotFound(cursor);
            return toObject.Invoke(list);
        }
    }
}
