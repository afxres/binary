namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Adapters;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using System.Runtime.CompilerServices;

internal sealed class NativeEndianConverter<T> : ConstantConverter<T, NativeEndianConverter<T>.Functions> where T : unmanaged
{
    internal readonly struct Functions : IConstantConverterFunctions<T>, ISpanLikeEncoderProvider<T>, ISpanLikeDecoderProvider<T[]>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        public static void Encode(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);

        SpanLikeEncoder<T> ISpanLikeEncoderProvider<T>.GetEncoder() => new NativeEndianEncoder<T>();

        SpanLikeDecoder<T[]> ISpanLikeDecoderProvider<T[]>.GetDecoder() => new NativeEndianDecoder<T>();
    }
}
