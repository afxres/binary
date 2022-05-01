namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features.Contexts;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal readonly struct GuidRawConverter : IRawConverter<Guid>
{
    public static int Length => Unsafe.SizeOf<Guid>();

    public static Guid Decode(ref byte source) => new Guid(MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.SizeOf<Guid>()));

    public static void Encode(ref byte target, Guid item) => _ = item.TryWriteBytes(MemoryMarshal.CreateSpan(ref target, Unsafe.SizeOf<Guid>()));
}
#endif
