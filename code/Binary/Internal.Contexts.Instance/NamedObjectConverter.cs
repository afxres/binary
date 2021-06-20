using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Template;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Contexts.Instance
{
    internal delegate T NamedObjectDecodeDelegate<out T>(ReadOnlySpan<byte> span, ReadOnlySpan<long> data);

    internal sealed class NamedObjectConverter<T> : Converter<T>
    {
        private readonly int capacity;

        private readonly ImmutableArray<string> names;

        private readonly ByteViewDictionary<int> dictionary;

        private readonly EncodeDelegate<T> encode;

        private readonly NamedObjectDecodeDelegate<T> decode;

        public NamedObjectConverter(EncodeDelegate<T> encode, NamedObjectDecodeDelegate<T> decode, ImmutableArray<string> names, ByteViewDictionary<int> dictionary)
        {
            Debug.Assert(dictionary is not null);
            Debug.Assert(names.Any());
            this.names = names;
            this.capacity = names.Length;
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
                    handle = NamedObjectTemplates.GetIndexData(offset, length);
                    remain--;
                }
            }

            if (remain is not 0)
                return ExceptNotFound(values.IndexOf(0));
            return decode.Invoke(span, values);
        }
    }
}
