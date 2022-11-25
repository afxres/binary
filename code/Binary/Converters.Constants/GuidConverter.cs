namespace Mikodev.Binary.Converters.Constants;

using Mikodev.Binary.Features.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class GuidConverter : ConstantConverter<Guid, GuidConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<Guid>
    {
        public static int Length => Unsafe.SizeOf<Guid>();

        public static Guid Decode(ref byte source)
        {
            return new Guid(MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.SizeOf<Guid>()));
        }

        public static void Encode(ref byte target, Guid item)
        {
            var buffer = MemoryMarshal.CreateSpan(ref target, Unsafe.SizeOf<Guid>());
            var result = item.TryWriteBytes(buffer);
            Debug.Assert(result);
        }
    }
}
