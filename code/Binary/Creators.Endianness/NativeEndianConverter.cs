namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Adapters;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal sealed class NativeEndianConverter<T> : ConstantConverter<T, NativeEndianConverter<T>.Functions> where T : unmanaged
{
    internal readonly struct Functions : IConstantConverterFunctions<T>, ISpanLikeContextProvider<T>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        public static void Encode(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);

        SpanLikeForwardEncoder<T> ISpanLikeContextProvider<T>.GetEncoder() => new NativeEndianEncoder<T>();

        SpanLikeDecoder<T[]> ISpanLikeContextProvider<T>.GetDecoder() => new NativeEndianDecoder<T>();

        SpanLikeDecoder<List<T>> ISpanLikeContextProvider<T>.GetListDecoder() => new NativeEndianListDecoder<T>();
    }
}
