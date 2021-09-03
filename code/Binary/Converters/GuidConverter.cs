namespace Mikodev.Binary.Converters;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class GuidConverter : Converter<Guid>
{
    public GuidConverter() : base(Unsafe.SizeOf<Guid>()) { }

    public override void Encode(ref Allocator allocator, Guid item)
    {
        _ = item.TryWriteBytes(MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<Guid>()), Unsafe.SizeOf<Guid>()));
    }

    public override Guid Decode(in ReadOnlySpan<byte> span)
    {
        return new Guid(span);
    }
}
