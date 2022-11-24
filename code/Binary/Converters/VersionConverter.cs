namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class VersionConverter : VariableWriterEncodeConverter<Version?, VersionConverter.Functions>
{
    internal struct Functions : IVariableWriterEncodeConverterFunctions<Version?>
    {
        private const int MaxLength = 16;

        public static int GetMaxLength(Version? item)
        {
            return MaxLength;
        }

        public static int Encode(Span<byte> span, Version? item)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Encode(Span<byte> span, int offset, int data)
            {
                BinaryPrimitives.WriteInt32LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), offset * sizeof(int)), sizeof(int)), data);
            }

            if (span.Length < MaxLength)
                ThrowHelper.ThrowTryWriteBytesFailed();
            if (item is null)
                return 0;
            Encode(span, 0, item.Major);
            Encode(span, 1, item.Minor);
            if (item.Build is -1)
                return 8;
            Encode(span, 2, item.Build);
            if (item.Revision is -1)
                return 12;
            Encode(span, 3, item.Revision);
            return 16;
        }

        public static Version? Decode(in ReadOnlySpan<byte> span)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Decode(ReadOnlySpan<byte> span, int offset)
            {
                return BinaryPrimitives.ReadInt32LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), offset * sizeof(int)), sizeof(int)));
            }

            var item = default(Version?);
            if (span.Length is 8)
                item = new Version(Decode(span, 0), Decode(span, 1));
            else if (span.Length is 12)
                item = new Version(Decode(span, 0), Decode(span, 1), Decode(span, 2));
            else if (span.Length is 16)
                item = new Version(Decode(span, 0), Decode(span, 1), Decode(span, 2), Decode(span, 3));
            else if (span.Length is not 0)
                ThrowHelper.ThrowNotEnoughBytes();
            return item;
        }
    }
}
