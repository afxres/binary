﻿namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

internal sealed class VersionConverter : VariableConverter<Version?, VersionConverter.Functions>
{
    private const int MaxLength = 16;

    private static readonly AllocatorWriter<Version?> EncodeFunction;

    static VersionConverter()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Encode(Span<byte> span, int offset, int data)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset * sizeof(int), sizeof(int)), data);
        }

        static int Invoke(Span<byte> span, Version? item)
        {
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
        EncodeFunction = Invoke;
    }

    private static Version? DecodeInternal(ReadOnlySpan<byte> source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Decode(ReadOnlySpan<byte> span, int offset)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset * sizeof(int), sizeof(int)));
        }

        var result = default(Version?);
        if (source.Length is 8)
            result = new Version(Decode(source, 0), Decode(source, 1));
        else if (source.Length is 12)
            result = new Version(Decode(source, 0), Decode(source, 1), Decode(source, 2));
        else if (source.Length is 16)
            result = new Version(Decode(source, 0), Decode(source, 1), Decode(source, 2), Decode(source, 3));
        else if (source.Length is not 0)
            ThrowHelper.ThrowNotEnoughBytes();
        return result;
    }

    internal readonly struct Functions : IVariableConverterFunctions<Version?>
    {
        public static Version? Decode(in ReadOnlySpan<byte> span)
        {
            return DecodeInternal(span);
        }

        public static void Encode(ref Allocator allocator, Version? item)
        {
            Allocator.Append(ref allocator, MaxLength, item, EncodeFunction);
        }

        public static void EncodeWithLengthPrefix(ref Allocator allocator, Version? item)
        {
            Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeFunction);
        }
    }
}
