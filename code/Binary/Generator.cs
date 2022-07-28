namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Internal.Contexts;
using System.Collections.Immutable;

public static class Generator
{
    private static readonly ImmutableArray<IConverterCreator> SharedConverterCreators;

    static Generator()
    {
        var creators = new IConverterCreator[]
        {
            new KeyValuePairConverterCreator(),
            new LinkedListConverterCreator(),
            new NullableConverterCreator(),
            new UriConverterCreator(),
#if NET6_0_OR_GREATER
            new PriorityQueueConverterCreator(),
#endif
        };
        SharedConverterCreators = creators.ToImmutableArray();
    }

    public static IGenerator CreateDefault()
    {
        return CreateDefaultBuilder().Build();
    }

    public static IGeneratorBuilder CreateDefaultBuilder()
    {
        var builder = new GeneratorBuilder();
        foreach (var creator in SharedConverterCreators)
            _ = builder.AddConverterCreator(creator);
#if NET7_0_OR_GREATER
        builder.AddConverterCreator(new Features.RawConverterCreator());
#endif
        return builder;
    }
}
