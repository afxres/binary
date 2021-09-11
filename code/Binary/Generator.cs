namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System.Collections.Immutable;
using System.Linq;

public static class Generator
{
    private static readonly ImmutableArray<IConverterCreator> SharedConverterCreators;

    static Generator()
    {
        var creators = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.Namespace is "Mikodev.Binary.Creators" && typeof(IConverterCreator).IsAssignableFrom(x))
            .Select(x => (IConverterCreator)CommonHelper.CreateInstance(x, null))
            .ToImmutableArray();
        SharedConverterCreators = creators;
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
        return builder;
    }
}
