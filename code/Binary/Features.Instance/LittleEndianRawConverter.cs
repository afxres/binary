namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Features.Fallback;
using System.Runtime.CompilerServices;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal readonly struct LittleEndianRawConverter<T> : IRawConverter<T> where T : unmanaged
{
    public static int Length => Unsafe.SizeOf<T>();

    public static T Decode(ref byte source) => LittleEndianFallback.Decode<T>(ref source);

    public static void Encode(ref byte target, T item) => LittleEndianFallback.Encode(ref target, item);
}
