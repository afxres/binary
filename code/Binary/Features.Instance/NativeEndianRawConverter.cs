namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Converters.Endianness.Adapters;
using Mikodev.Binary.Features;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Contexts;
using System.Runtime.CompilerServices;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal readonly struct NativeEndianRawConverter<T> : IRawConverter<T>, ISequenceAdapterCreator<T> where T : unmanaged
{
    public static int Length => Unsafe.SizeOf<T>();

    public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

    public static void Encode(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);

    SequenceAdapter<T> ISequenceAdapterCreator<T>.GetAdapter() => new NativeEndianSequenceAdapter<T>();
}
