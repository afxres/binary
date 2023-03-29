namespace Mikodev.Binary;

using System;
using System.Collections.Generic;

public static class GeneratorBuilderExtensions
{
    public static IGeneratorBuilder AddConverters(this IGeneratorBuilder builder, IEnumerable<IConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(converters);
        foreach (var converter in converters)
            builder = builder.AddConverter(converter);
        return builder;
    }

    public static IGeneratorBuilder AddConverterCreators(this IGeneratorBuilder builder, IEnumerable<IConverterCreator> creators)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(creators);
        foreach (var creator in creators)
            builder = builder.AddConverterCreator(creator);
        return builder;
    }
}
