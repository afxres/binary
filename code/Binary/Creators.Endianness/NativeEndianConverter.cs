namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Adapters;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Contexts;
using System.Runtime.CompilerServices;

internal sealed class NativeEndianConverter<T> : ConstantConverter<T, NativeEndianConverter<T>.Functions> where T : unmanaged
{
    internal readonly struct Functions : IConstantConverterFunctions<T>, ISequenceAdapterCreator<T>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        public static void Encode(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);

        SequenceAdapter<T> ISequenceAdapterCreator<T>.GetAdapter() => new NativeEndianAdapter<T>();
    }
}
