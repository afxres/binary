using Mikodev.Binary.Collections;
using Mikodev.Binary.Unions;
using System;

namespace Mikodev.Binary
{
    public static class GeneratorBuilderFSharpExtensions
    {
        public static IGeneratorBuilder AddFSharpConverterCreators(this IGeneratorBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            _ = builder.AddConverterCreator(new FSharpCollectionConverterCreator());
            _ = builder.AddConverterCreator(new UnionConverterCreator());
            return builder;
        }
    }
}
