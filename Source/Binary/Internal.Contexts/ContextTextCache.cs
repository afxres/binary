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
            var encoding = Converter.Encoding;
            var buffer = encoding.GetBytes(text);
            var length = buffer.Length;
            var target = new byte[length + sizeof(int)];
            Endian<int>.Set(ref target[0], length);
            Memory.Copy(ref target[sizeof(int)], ref buffer[0], length);
            result = (buffer, target);
            dictionary.Add(text, result);
            return result;
        }

        public byte[] GetBuffer(string text) => GetBufferResult(text).Buffer;

        public byte[] GetBufferWithLengthPrefix(string text) => GetBufferResult(text).BufferWithLengthPrefix;
    }
}
