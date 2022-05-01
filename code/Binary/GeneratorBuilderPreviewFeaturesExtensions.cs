namespace Mikodev.Binary;

using Mikodev.Binary.Features;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
public static class GeneratorBuilderPreviewFeaturesExtensions
{
    public static IGeneratorBuilder AddPreviewFeaturesConverterCreators(this IGeneratorBuilder builder)
    {
        return builder.AddConverterCreator(new RawConverterCreator());
    }
}
#endif
