namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

internal sealed class VersionConverter : VariableConverter<Version?, VersionConverter.Functions>
{
    internal readonly struct Functions : IVariableConverterFunctions<Version?>
    {
        public static int Limits(Version? item)
        {
            return item is null ? 0 : 16;
        }

        public static int Append(Span<byte> span, Version? item)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Encode(Span<byte> span, int offset, int data)
            {
                BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset * sizeof(int), sizeof(int)), data);
            }

            if (span.Length < 16)
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

        public static Version? Decode(in ReadOnlySpan<byte> source)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Decode(ReadOnlySpan<byte> span, int offset)
            {
                return BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset * sizeof(int), sizeof(int)));
            }

            if (source.Length is 8)
                return new Version(Decode(source, 0), Decode(source, 1));
            if (source.Length is 12)
                return new Version(Decode(source, 0), Decode(source, 1), Decode(source, 2));
            if (source.Length is 16)
                return new Version(Decode(source, 0), Decode(source, 1), Decode(source, 2), Decode(source, 3));
            if (source.Length is not 0)
                ThrowHelper.ThrowNotEnoughBytes();
            return null;
        }
    }
}
