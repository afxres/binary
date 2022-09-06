namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Legacies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Generator
{
    private static readonly ConcurrentDictionary<string, IConverterCreator> SharedConverterCreators;

    static Generator()
    {
        var creators = new List<IConverterCreator>
        {
            new KeyValuePairConverterCreator(),
            new LinkedListConverterCreator(),
            new NullableConverterCreator(),
            new PriorityQueueConverterCreator(),
            new UriConverterCreator(),
        };
        SharedConverterCreators = new ConcurrentDictionary<string, IConverterCreator>(creators.ToDictionary(x => x.GetType().Name));
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    static IConverterCreator Create(string featureName, string typeName)
    {
        if (RuntimeFeature.IsSupported(featureName) is false)
            return new OldConverterCreator();
        var type = typeof(IConverter).Assembly.GetType(typeName);
        var item = type is null ? null : Activator.CreateInstance(type);
        if (item is null)
            throw new ArgumentException($"Create instance error, type: {typeName}");
        return (IConverterCreator)item;
    }

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static IGenerator CreateDefault()
    {
        return CreateDefaultBuilder().Build();
    }

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public static IGeneratorBuilder CreateDefaultBuilder()
    {
        const string Key = "ConverterCreator";
        var dictionary = SharedConverterCreators;
        if (dictionary.ContainsKey(Key) is false)
            _ = dictionary.TryAdd(Key, Create("VirtualStaticsInInterfaces", "Mikodev.Binary.Features.RawConverterCreator"));
        var builder = new GeneratorBuilder();
        foreach (var creator in dictionary.Values)
            _ = builder.AddConverterCreator(creator);
        return builder;
    }
}
