namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Contexts;
using System.Runtime.CompilerServices;

internal sealed class NativeEndianConverter<T> : ConstantConverter<T, NativeEndianConverter<T>.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<T>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        public static void Encode(ref byte target, T? item) => Unsafe.WriteUnaligned(ref target, item);
    }
}
