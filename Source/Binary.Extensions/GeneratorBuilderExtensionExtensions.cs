using Mikodev.Binary.Creators.Collections;
using Mikodev.Binary.Creators.Tuples;
using Mikodev.Binary.Creators.Unions;
using System;

namespace Mikodev.Binary
{
    public static class GeneratorBuilderExtensionExtensions
    {
        public static IGeneratorBuilder AddExtensionConverterCreators(this IGeneratorBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            _ = builder.AddConverterCreator(new FSharpCollectionConverterCreator());
            _ = builder.AddConverterCreator(new UnionConverterCreator());
            _ = builder.AddConverterCreator(new TupleLikeConverterCreator());
            return builder;
        }
    }
}
