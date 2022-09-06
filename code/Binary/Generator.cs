namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Legacies;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static class Generator
{
    private static ImmutableArray<IConverterCreator> SharedConverterCreators;

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static ImmutableArray<IConverterCreator> CreateConverterCreators()
    {
        var creators = new List<IConverterCreator>()
        {
            new KeyValuePairConverterCreator(),
            new LinkedListConverterCreator(),
            new NullableConverterCreator(),
            new PriorityQueueConverterCreator(),
            new UriConverterCreator(),
        };

        static object Create(string featureName, string typeName)
        {
            if (!RuntimeFeature.IsSupported(featureName))
                return new OldConverterCreator();
            var type = typeof(IConverter).Assembly.GetType(typeName);
            var item = type is null ? null : Activator.CreateInstance(type);
            if (item is null)
                throw new ArgumentException($"Create instance error, type: {typeName}");
            return item;
        }

        creators.Add((IConverterCreator)Create("VirtualStaticsInInterfaces", "Mikodev.Binary.Features.RawConverterCreator"));
        return creators.ToImmutableArray();
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
        var caches = SharedConverterCreators;
        if (caches.IsDefaultOrEmpty)
            SharedConverterCreators = caches = CreateConverterCreators();
        var builder = new GeneratorBuilder();
        foreach (var creator in caches)
            _ = builder.AddConverterCreator(creator);
        return builder;
    }
}
