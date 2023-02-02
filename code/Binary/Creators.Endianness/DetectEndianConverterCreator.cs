namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

internal sealed class DetectEndianConverterCreator : IConverterCreator
{
    private static readonly ImmutableDictionary<Type, (IConverter Little, IConverter Native)> SharedConverters;

    static DetectEndianConverterCreator()
    {
        var little = new IConverter[]
        {
            new LittleEndianConverter<bool>(),
            new LittleEndianConverter<byte>(),
            new LittleEndianConverter<sbyte>(),
            new LittleEndianConverter<char>(),
            new LittleEndianConverter<short>(),
            new LittleEndianConverter<int>(),
            new LittleEndianConverter<long>(),
            new LittleEndianConverter<ushort>(),
            new LittleEndianConverter<uint>(),
            new LittleEndianConverter<ulong>(),
            new LittleEndianConverter<float>(),
            new LittleEndianConverter<double>(),
            new LittleEndianConverter<Half>(),
            new LittleEndianConverter<BitVector32>(),
            new LittleEndianConverter<Int128>(),
            new LittleEndianConverter<UInt128>(),
        };

        var native = new IConverter[]
        {
            new NativeEndianConverter<bool>(),
            new NativeEndianConverter<byte>(),
            new NativeEndianConverter<sbyte>(),
            new NativeEndianConverter<char>(),
            new NativeEndianConverter<short>(),
            new NativeEndianConverter<int>(),
            new NativeEndianConverter<long>(),
            new NativeEndianConverter<ushort>(),
            new NativeEndianConverter<uint>(),
            new NativeEndianConverter<ulong>(),
            new NativeEndianConverter<float>(),
            new NativeEndianConverter<double>(),
            new NativeEndianConverter<Half>(),
            new NativeEndianConverter<BitVector32>(),
            new NativeEndianConverter<Int128>(),
            new NativeEndianConverter<UInt128>(),
        };

        Debug.Assert(little.Length == native.Length);
        Debug.Assert(little.Select(Converter.GetGenericArgument).SequenceEqual(native.Select(Converter.GetGenericArgument)));
        SharedConverters = little.Zip(native, (a, b) => (Little: a, b)).ToImmutableDictionary(x => Converter.GetGenericArgument(x.Little));
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (SharedConverters.TryGetValue(type, out var result))
                return native ? result.Native : result.Little;
            if (type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianConverter<>)
                : typeof(LittleEndianConverter<>);
            var converterType = definition.MakeGenericType(type);
            var converter = CommonModule.CreateInstance(converterType, null);
            return (IConverter)converter;
        }

        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
