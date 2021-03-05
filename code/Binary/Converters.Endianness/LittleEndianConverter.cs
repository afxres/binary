using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Endianness
{
    internal sealed class LittleEndianConverter<T> : Converter<T> where T : unmanaged
    {
        public LittleEndianConverter() : base(Unsafe.SizeOf<T>())
        {
            Debug.Assert(Unsafe.SizeOf<T>() <= 8 || typeof(T) == typeof(Guid));
            Debug.Assert(Unsafe.SizeOf<T>() is 1 or 2 or 4 or 8 or 16);
            Debug.Assert(MemoryHelper.EncodeNumberLength((uint)Unsafe.SizeOf<T>()) is 1);
        }

        private static void EncodeInternal(ref byte location, T item)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static E MakeCast<E>(T data) => Unsafe.As<T, E>(ref data);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

            if (typeof(T) == typeof(Guid))
                _ = ((Guid)(object)item).TryWriteBytes(MakeSpan(ref location));
            else if (Unsafe.SizeOf<T>() is 1)
                Unsafe.WriteUnaligned(ref location, MakeCast<byte>(item));
            else if (Unsafe.SizeOf<T>() is 2)
                BinaryPrimitives.WriteInt16LittleEndian(MakeSpan(ref location), MakeCast<short>(item));
            else if (Unsafe.SizeOf<T>() is 4)
                BinaryPrimitives.WriteInt32LittleEndian(MakeSpan(ref location), MakeCast<int>(item));
            else if (Unsafe.SizeOf<T>() is 8)
                BinaryPrimitives.WriteInt64LittleEndian(MakeSpan(ref location), MakeCast<long>(item));
            else
                throw new NotSupportedException();
        }

        private static T DecodeInternal(ref byte location)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T MakeCast<E>(E data) => Unsafe.As<E, T>(ref data);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

            if (typeof(T) == typeof(Guid))
                return (T)(object)new Guid(MakeSpan(ref location));
            else if (Unsafe.SizeOf<T>() is 1)
                return MakeCast(Unsafe.ReadUnaligned<byte>(ref location));
            else if (Unsafe.SizeOf<T>() is 2)
                return MakeCast(BinaryPrimitives.ReadInt16LittleEndian(MakeSpan(ref location)));
            else if (Unsafe.SizeOf<T>() is 4)
                return MakeCast(BinaryPrimitives.ReadInt32LittleEndian(MakeSpan(ref location)));
            else if (Unsafe.SizeOf<T>() is 8)
                return MakeCast(BinaryPrimitives.ReadInt64LittleEndian(MakeSpan(ref location)));
            else
                throw new NotSupportedException();
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            EncodeInternal(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            return DecodeInternal(ref MemoryHelper.EnsureLength(span, Unsafe.SizeOf<T>()));
        }
    }
}
