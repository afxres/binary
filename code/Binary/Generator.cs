namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Contexts;
using System;
using System.Linq;

public static class Generator
{
    public static IGenerator CreateDefault()
    {
        return CreateDefaultBuilder().Build();
    }

    public static IGeneratorBuilder CreateDefaultBuilder()
    {
        var creators = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsAbstract is false && typeof(IConverterCreator).IsAssignableFrom(x))
            .Select(x => (IConverterCreator)Activator.CreateInstance(x))
            .ToList();
        var builder = new GeneratorBuilder();
        foreach (var creator in creators)
            _ = builder.AddConverterCreator(creator);
        return builder;
    }
}
