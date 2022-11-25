namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Features.Fallback;
using System.Runtime.CompilerServices;

internal sealed class LittleEndianConverter<T> : ConstantConverter<T, LittleEndianConverter<T>.Functions> where T : unmanaged
{
    internal readonly struct Functions : IConstantConverterFunctions<T>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source) => LittleEndianFallback.Decode<T>(ref source);

        public static void Encode(ref byte target, T item) => LittleEndianFallback.Encode(ref target, item);
    }
}
