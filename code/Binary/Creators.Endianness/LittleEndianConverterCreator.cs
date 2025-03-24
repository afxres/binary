namespace Mikodev.Binary.Creators.Endianness;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;

internal sealed class LittleEndianConverterCreator : IConverterCreator
{
    private static readonly FrozenDictionary<Type, IConverter> SharedConverters;

    static LittleEndianConverterCreator()
    {
        static void Register<T>(Dictionary<Type, IConverter> dictionary) where T : unmanaged
        {
            dictionary.Add(typeof(T), new LittleEndianConverter<T>());
        }

        static void RegisterRepeat<T, E>(Dictionary<Type, IConverter> dictionary) where T : unmanaged where E : unmanaged
        {
            dictionary.Add(typeof(T), new RepeatLittleEndianConverter<T, E>());
        }

        var dictionary = new Dictionary<Type, IConverter>();

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
        return SharedConverters.TryGetValue(type, out var result) ? result : null;
    }
}
