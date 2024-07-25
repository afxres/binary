namespace Mikodev.Binary.Creators.Endianness;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using ConverterPair = (IConverter Little, IConverter Native);

internal sealed class DetectEndianConverterCreator : IConverterCreator
{
    private static readonly FrozenDictionary<Type, ConverterPair> SharedConverters;

    static DetectEndianConverterCreator()
    {
        static void Register<T>(Dictionary<Type, ConverterPair> dictionary) where T : unmanaged
        {
            var little = new LittleEndianConverter<T>();
            var native = new NativeEndianConverter<T>();
            dictionary.Add(typeof(T), (little, native));
        }

        static void RegisterRepeat<T, E>(Dictionary<Type, ConverterPair> dictionary) where T : unmanaged where E : unmanaged
        {
            var little = new RepeatLittleEndianConverter<T, E>();
            var native = new NativeEndianConverter<T>();
            dictionary.Add(typeof(T), (little, native));
        }

        var dictionary = new Dictionary<Type, ConverterPair>();

        Register<bool>(dictionary);
        Register<byte>(dictionary);
        Register<sbyte>(dictionary);
        Register<char>(dictionary);
        Register<short>(dictionary);
        Register<int>(dictionary);
        Register<long>(dictionary);
        Register<ushort>(dictionary);
        Register<uint>(dictionary);
        Register<ulong>(dictionary);
        Register<float>(dictionary);
        Register<double>(dictionary);
        Register<Half>(dictionary);
        Register<Index>(dictionary);
        Register<BitVector32>(dictionary);
        Register<Int128>(dictionary);
        Register<UInt128>(dictionary);

        RegisterRepeat<Range, int>(dictionary);
        RegisterRepeat<Complex, double>(dictionary);
        RegisterRepeat<Matrix3x2, float>(dictionary);
        RegisterRepeat<Matrix4x4, float>(dictionary);
        RegisterRepeat<Plane, float>(dictionary);
        RegisterRepeat<Quaternion, float>(dictionary);
        RegisterRepeat<Vector2, float>(dictionary);
        RegisterRepeat<Vector3, float>(dictionary);
        RegisterRepeat<Vector4, float>(dictionary);

        SharedConverters = dictionary.ToFrozenDictionary();
    }

    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (SharedConverters.TryGetValue(type, out var result))
                return native ? result.Native : result.Little;
            return null;
        }

        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
