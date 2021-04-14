using Mikodev.Binary.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Contexts.Instance
{
    internal delegate void NamedObjectEncoder<in T>(ref Allocator allocator, T item);

    internal delegate T NamedObjectDecoder<out T>(in MemorySlices list);

    internal sealed class NamedObjectConverter<T> : Converter<T>
    {
        private readonly int capacity;

        private readonly IReadOnlyList<string> names;

        private readonly BinaryDictionary<int> dictionary;

        private readonly NamedObjectEncoder<T> encode;

        private readonly NamedObjectDecoder<T> decode;

        public NamedObjectConverter(NamedObjectEncoder<T> encode, NamedObjectDecoder<T> decode, IReadOnlyCollection<string> names, BinaryDictionary<int> dictionary)
        {
            Debug.Assert(dictionary is not null);
            Debug.Assert(names.Any());
            this.names = names.ToArray();
            this.capacity = names.Count;
            this.dictionary = dictionary;
            this.encode = encode;
            this.decode = decode;
        }

        [DebuggerStepThrough, DoesNotReturn]
        private T ExceptKeyFound(int i) => throw new ArgumentException($"Named key '{this.names[i]}' already exists, type: {typeof(T)}");

        [DebuggerStepThrough, DoesNotReturn]
        private T ExceptNotFound(int i) => throw new ArgumentException($"Named key '{this.names[i]}' does not exist, type: {typeof(T)}");

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            this.encode.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            var decode = this.decode;
            if (decode is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            if (span.Length is 0 && default(T) is null)
                return default;
            if (span.Length is 0 && default(T) is not null)
                ThrowHelper.ThrowNotEnoughBytes();

            // maybe 'StackOverflowException', just let it crash
            var remain = this.capacity;
            var record = this.dictionary;
            var values = (stackalloc long[remain]);
            ref var source = ref MemoryMarshal.GetReference(span);

            var limits = span.Length;
            var offset = 0;
            var length = 0;
            while (limits - offset != length)
            {
                offset += length;
                length = NumberHelper.DecodeEnsureBuffer(ref source, ref offset, limits);
                var cursor = record.GetValue(ref Unsafe.Add(ref source, offset), length);
                Debug.Assert(cursor is -1 || (uint)cursor < (uint)values.Length);
                offset += length;
                length = NumberHelper.DecodeEnsureBuffer(ref source, ref offset, limits);
                if ((uint)cursor < (uint)values.Length)
                {
                    ref var handle = ref values[cursor];
                    if (handle is not 0)
                        return ExceptKeyFound(cursor);
                    handle = (long)(((ulong)(uint)offset << 32) | (uint)length);
                    remain--;
                }
            }

            if (remain is not 0)
                return ExceptNotFound(values.IndexOf(0));
            return decode.Invoke(new MemorySlices(span, values));
        }
    }
}
