namespace Mikodev.Binary.Creators.Endianness;

using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Numerics;

internal sealed class DetectEndianConverterCreator : IConverterCreator
{
    private static readonly ImmutableDictionary<Type, (IConverter Little, IConverter Native)> SharedConverters;

    static DetectEndianConverterCreator()
    {
        static void Register<T>(ImmutableDictionary<Type, (IConverter, IConverter)>.Builder builder) where T : unmanaged
        {
            var little = new LittleEndianConverter<T>();
            var native = new NativeEndianConverter<T>();
            builder.Add(typeof(T), (little, native));
        }

        static void RegisterRepeat<T, E>(ImmutableDictionary<Type, (IConverter, IConverter)>.Builder builder) where T : unmanaged where E : unmanaged
        {
            var little = new RepeatLittleEndianConverter<T, E>();
            var native = new NativeEndianConverter<T>();
            builder.Add(typeof(T), (little, native));
        }

        var builder = ImmutableDictionary.CreateBuilder<Type, (IConverter, IConverter)>();

        Register<bool>(builder);
        Register<byte>(builder);
        Register<sbyte>(builder);
        Register<char>(builder);
        Register<short>(builder);
        Register<int>(builder);
        Register<long>(builder);
        Register<ushort>(builder);
        Register<uint>(builder);
        Register<ulong>(builder);
        Register<float>(builder);
        Register<double>(builder);
        Register<Half>(builder);
        Register<BitVector32>(builder);
        Register<Int128>(builder);
        Register<UInt128>(builder);

        RegisterRepeat<Complex, double>(builder);
        RegisterRepeat<Matrix3x2, float>(builder);
        RegisterRepeat<Matrix4x4, float>(builder);
        RegisterRepeat<Plane, float>(builder);
        RegisterRepeat<Quaternion, float>(builder);
        RegisterRepeat<Vector2, float>(builder);
        RegisterRepeat<Vector3, float>(builder);
        RegisterRepeat<Vector4, float>(builder);

        SharedConverters = builder.ToImmutable();
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
