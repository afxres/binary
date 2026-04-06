namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Buffers;

public abstract partial class Converter<T>
{
    private byte[] EncodeConstant(T? item)
    {
        var length = this.length;
        var buffer = new byte[length];
        var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
        Encode(ref allocator, item);
        return buffer;
    }

    private byte[] EncodeVariable(T? item)
    {
        using var underlying = new ArrayPoolAllocator(ArrayPool<byte>.Shared);
        var allocator = new Allocator(underlying);
        Encode(ref allocator, item);
        return allocator.ToArray();
    }

    private void EncodeWithLengthPrefixConstant(ref Allocator allocator, T? item)
    {
        var length = this.length;
        var numberLength = NumberModule.EncodeLength((uint)length);
        NumberModule.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
        Encode(ref allocator, item);
    }

    private void EncodeWithLengthPrefixVariable(ref Allocator allocator, T? item)
    {
        var anchor = Allocator.Anchor(ref allocator);
        Encode(ref allocator, item);
        Allocator.FinishAnchor(ref allocator, anchor);
    }
}
