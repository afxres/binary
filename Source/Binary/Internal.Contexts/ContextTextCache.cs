using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class ContextTextCache
    {
        private readonly Dictionary<string, (byte[], byte[])> dictionary = new Dictionary<string, (byte[], byte[])>();

        private (byte[] Buffer, byte[] BufferWithLengthPrefix) GetBufferResult(string text)
        {
            if (dictionary.TryGetValue(text, out var result))
                return result;
            Debug.Assert(!string.IsNullOrEmpty(text));
            var buffer = Converter.Encoding.GetBytes(text);
            var allocator = new Allocator();
            PrimitiveHelper.EncodeWithLengthPrefix(ref allocator, buffer);
            var target = allocator.ToArray();
            result = (buffer, target);
            dictionary.Add(text, result);
            return result;
        }

        public byte[] GetBuffer(string text) => GetBufferResult(text).Buffer;

        public byte[] GetBufferWithLengthPrefix(string text) => GetBufferResult(text).BufferWithLengthPrefix;
    }
}
