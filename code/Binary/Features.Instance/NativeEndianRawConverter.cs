namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features.Contexts;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct NativeEndianRawConverter<T> : IRawConverter<T> where T : unmanaged
{
    public static int Length => Unsafe.SizeOf<T>();

    public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

    public static void Encode(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);
}
#endif
