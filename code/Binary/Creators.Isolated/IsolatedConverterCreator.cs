namespace Mikodev.Binary.Creators.Isolated;

using Mikodev.Binary.Creators.Isolated.Constants;
using Mikodev.Binary.Creators.Isolated.Primitive;
using Mikodev.Binary.Creators.Isolated.Variables;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

internal sealed class IsolatedConverterCreator : IConverterCreator
{
    private static readonly FrozenDictionary<Type, IConverter> SharedConverters = GetConverters().ToFrozenDictionary(Converter.GetGenericArgument);

    private static IEnumerable<IConverter> GetConverters()
    {
        yield return new DateOnlyConverter();
        yield return new DateTimeConverter();
        yield return new DateTimeOffsetConverter();
        yield return new DecimalConverter();
        yield return new GuidConverter();
        yield return new RuneConverter();
        yield return new TimeOnlyConverter();
        yield return new TimeSpanConverter();
        yield return new BigIntegerConverter();
        yield return new BitArrayConverter();
        yield return new IPAddressConverter();
        yield return new IPEndPointConverter();
        yield return new VersionConverter();
        yield return new StringConverter();
    }

    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return SharedConverters.GetValueOrDefault(type);
    }
}
