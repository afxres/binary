using Mikodev.Binary.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal delegate void OfNamedObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToNamedObject<out T>(LengthList list);

    internal sealed class NamedObjectConverter<T> : Converter<T>
    {
        private readonly OfNamedObject<T> encode;

        private readonly ToNamedObject<T> decode;

        private readonly BinaryNode<int> entry;

        private readonly string[] names;

        public NamedObjectConverter(OfNamedObject<T> encode, ToNamedObject<T> decode, BinaryNode<int> entry, IReadOnlyCollection<string> names)
        {
            Debug.Assert(entry != null);
            Debug.Assert(names != null && names.Any());
            this.encode = encode;
            this.decode = decode;
            this.entry = entry;
            this.names = names.ToArray();
        }

        [DebuggerStepThrough, DoesNotReturn]
        private T ThrowKeyFound(int i) => throw new ArgumentException($"Named key '{names[i]}' already exists, type: {ItemType}");

        [DebuggerStepThrough, DoesNotReturn]
        private T ThrowNotFound(int i) => throw new ArgumentException($"Named key '{names[i]}' does not exist, type: {ItemType}");

        private static void DetachPrefix(ref byte location, ref int offset, ref int length, int limits)
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

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            encode.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (decode is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var byteLength = span.Length;
            if (byteLength == 0)
                return default(T) is null ? default : ThrowHelper.ThrowNotEnoughBytes<T>();

            const int Limits = 32;
            var capacity = names.Length;
            var data = capacity > Limits ? new LengthItem[capacity] : stackalloc LengthItem[capacity];
            var list = new LengthList(span, data);
            ref var source = ref MemoryMarshal.GetReference(span);

            var limits = byteLength;
            var offset = 0;
            var length = 0;
            while (limits - offset != length)
            {
                DetachPrefix(ref source, ref offset, ref length, limits);
                var result = BinaryNodeHelper.GetOrDefault(entry, ref Unsafe.Add(ref source, offset), length);
                DetachPrefix(ref source, ref offset, ref length, limits);
                if (result is null || list.Insert(result.Value, offset, length))
                    continue;
                return ThrowKeyFound(result.Value);
            }

            var cursor = list.Ensure();
            if (cursor != -1)
                return ThrowNotFound(cursor);
            return decode.Invoke(list);
        }
    }
}
