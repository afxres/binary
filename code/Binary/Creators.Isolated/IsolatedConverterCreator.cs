namespace Mikodev.Binary.Creators.Isolated;

using Mikodev.Binary.Creators.Isolated.Constants;
using Mikodev.Binary.Creators.Isolated.Primitive;
using Mikodev.Binary.Creators.Isolated.Variables;
using System;
using System.Collections.Immutable;

internal sealed class IsolatedConverterCreator : IConverterCreator
{
    private static readonly ImmutableDictionary<Type, IConverter> SharedConverters;

    static IsolatedConverterCreator()
    {
        var converters = new IConverter[]
        {
            new DateOnlyConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new GuidConverter(),
            new RuneConverter(),
            new TimeOnlyConverter(),
            new TimeSpanConverter(),
            new BigIntegerConverter(),
            new IPAddressConverter(),
            new IPEndPointConverter(),
            new VersionConverter(),
            new StringConverter(),
        };
        SharedConverters = converters.ToImmutableDictionary(Converter.GetGenericArgument);
    }

    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
