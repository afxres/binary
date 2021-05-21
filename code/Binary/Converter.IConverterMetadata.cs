using Mikodev.Binary.Internal.Metadata;
using System;
using System.Reflection;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : IConverterMetadata
    {
        private delegate T DecodeReadOnlyDefine(in ReadOnlySpan<byte> span);

        private delegate T DecodeDefine(ref ReadOnlySpan<byte> span);

        private delegate void EncodeDefine(ref Allocator allocator, T item);

        Type IConverterMetadata.GetGenericArgument()
        {
            return typeof(T);
        }

        MethodInfo IConverterMetadata.GetMethodInfo(string methodName)
        {
            return methodName switch
            {
                nameof(Decode) => new DecodeReadOnlyDefine(Decode).Method,
                nameof(DecodeAuto) => new DecodeDefine(DecodeAuto).Method,
                nameof(DecodeWithLengthPrefix) => new DecodeDefine(DecodeWithLengthPrefix).Method,
                nameof(Encode) => new EncodeDefine(Encode).Method,
                nameof(EncodeAuto) => new EncodeDefine(EncodeAuto).Method,
                nameof(EncodeWithLengthPrefix) => new EncodeDefine(EncodeWithLengthPrefix).Method,
                _ => throw new ArgumentException($"Invalid method name."),
            };
        }
    }
}
