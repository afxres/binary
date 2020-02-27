using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendLittleEndian<T>(ref Allocator allocator, T item) where T : unmanaged
        {
            Debug.Assert(Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || Unsafe.SizeOf<T>() == 8);
            Ensure(ref allocator, Unsafe.SizeOf<T>());
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            var target = buffer.Slice(offset, Unsafe.SizeOf<T>());
            if (Unsafe.SizeOf<T>() == 2)
                BinaryPrimitives.WriteUInt16LittleEndian(target, Unsafe.As<T, ushort>(ref item));
            else if (Unsafe.SizeOf<T>() == 4)
                BinaryPrimitives.WriteUInt32LittleEndian(target, Unsafe.As<T, uint>(ref item));
            else
                BinaryPrimitives.WriteUInt64LittleEndian(target, Unsafe.As<T, ulong>(ref item));
            allocator.offset = offset + Unsafe.SizeOf<T>();
        }
    }
}
