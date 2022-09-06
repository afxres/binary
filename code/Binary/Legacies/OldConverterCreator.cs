namespace Mikodev.Binary.Legacies;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Legacies.Instance;
using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

internal sealed class OldConverterCreator : IConverterCreator
{
    private static readonly ImmutableArray<Type> Types = ImmutableArray.Create(new[]
{
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(Half),
        typeof(BitVector32),
    });

    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static OldConverterCreator()
    {
        var converters = new IConverter[]
        {
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DateOnlyConverter(),
            new DecimalConverter(),
            new GuidConverter(),
            new RuneConverter(),
            new TimeSpanConverter(),
            new TimeOnlyConverter(),
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (Types.Contains(type) is false && type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianConverter<>)
                : typeof(LittleEndianConverter<>);
            var converterType = definition.MakeGenericType(type);
            var converter = CommonModule.CreateInstance(converterType, null);
            return (IConverter)converter;
        }

        if (SharedConverters.TryGetValue(type, out var result))
            return result;
        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
