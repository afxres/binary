using Mikodev.Binary.Internal;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static class Converter
    {
        public static Type GetGenericArgument(IConverter converter)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));
            return GetGenericArgument(converter.GetType());
        }

        public static Type GetGenericArgument(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            var node = type;
            while ((node = node.BaseType) is not null)
                if (CommonHelper.TryGetGenericArguments(node, typeof(Converter<>), out var arguments))
                    return arguments.Single();
            throw new ArgumentException($"Can not get generic argument, '{type}' is not a subclass of '{typeof(Converter<>)}'");
        }

        public static void Encode(ref Allocator allocator, int number)
        {
            if (number < 0)
                ThrowHelper.ThrowNumberNegative();
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        }

        public static int Decode(ref ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryHelper.EnsureLength(span);
            var limits = span.Length;
            var offset = 0;
            var length = MemoryHelper.DecodeNumber(ref source, ref offset, limits);
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), limits - offset);
            return length;
        }

        public static void EncodeWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
            ref var target = ref Allocator.Assign(ref allocator, length + numberLength);
            MemoryHelper.EncodeNumber(ref target, (uint)length, numberLength);
            if (length is 0)
                return;
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref target, numberLength), ref MemoryMarshal.GetReference(span), (uint)length);
        }

        public static ReadOnlySpan<byte> DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryHelper.EnsureLength(span);
            var limits = span.Length;
            var offset = 0;
            var length = MemoryHelper.DecodeNumberEnsureBuffer(ref source, ref offset, limits);
            var cursor = offset + length;
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, cursor), limits - cursor);
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), length);
        }
    }
}
