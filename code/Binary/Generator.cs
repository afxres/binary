namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Internal.Contexts;
using System.Collections.Generic;
using System.Collections.Immutable;

public static class Generator
{
    private static readonly ImmutableDictionary<string, IConverterCreator> SharedConverterCreators;

    static Generator()
    {
        var creators = new List<IConverterCreator>
        {
            new KeyValuePairConverterCreator(),
            new LinkedListConverterCreator(),
            new NullableConverterCreator(),
            new PriorityQueueConverterCreator(),
            new UriConverterCreator(),
            new VariableBoundArrayConverterCreator(),
            new DetectEndianConverterCreator(),
        };
        SharedConverterCreators = creators.ToImmutableDictionary(x => x.GetType().Name);
    }

    public static IGenerator CreateDefault()
    {
        return CreateDefaultBuilder().Build();
    }

    public static IGeneratorBuilder CreateDefaultBuilder()
    {
        var builder = new GeneratorBuilder();
        foreach (var creator in SharedConverterCreators.Values)
            _ = builder.AddConverterCreator(creator);
        return builder;
    }
}
