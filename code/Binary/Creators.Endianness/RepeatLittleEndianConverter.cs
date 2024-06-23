namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Features.Fallback;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class RepeatLittleEndianConverter<T, E> : ConstantConverter<T, RepeatLittleEndianConverter<T, E>.Functions> where T : unmanaged where E : unmanaged
{
    internal readonly struct Functions : IConstantConverterFunctions<T>
    {
        public static int Length => Unsafe.SizeOf<T>();

        public static T Decode(ref byte source)
        {
            Debug.Assert(Unsafe.SizeOf<T>() > Unsafe.SizeOf<E>());
            Debug.Assert(Unsafe.SizeOf<T>() % Unsafe.SizeOf<E>() is 0);
            var length = Unsafe.SizeOf<T>() / Unsafe.SizeOf<E>();
            var result = default(T);
            var target = MemoryMarshal.CreateSpan(ref Unsafe.As<T, E>(ref result), length);
            for (var i = 0; i < target.Length; i++)
                target[i] = LittleEndianFallback.Decode<E>(ref Unsafe.Add(ref source, i * Unsafe.SizeOf<E>()));
            return result;
        }

        public static void Encode(ref byte target, T item)
        {
            Debug.Assert(Unsafe.SizeOf<T>() > Unsafe.SizeOf<E>());
            Debug.Assert(Unsafe.SizeOf<T>() % Unsafe.SizeOf<E>() is 0);
            var length = Unsafe.SizeOf<T>() / Unsafe.SizeOf<E>();
            var source = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, E>(ref item), length);
            for (var i = 0; i < source.Length; i++)
                LittleEndianFallback.Encode(ref Unsafe.Add(ref target, i * Unsafe.SizeOf<E>()), source[i]);
            return;
        }
    }
}
