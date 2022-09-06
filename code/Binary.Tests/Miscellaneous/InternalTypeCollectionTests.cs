namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

public class InternalTypeCollectionTests
{
    [Fact(DisplayName = "Fallback Converters Methods Shared Converters")]
    public void FallbackConvertersMethodsSharedConverters()
    {
        var expected = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.Namespace is "Mikodev.Binary.Converters")
            .Select(x => Assert.IsAssignableFrom<IConverter>(Activator.CreateInstance(x, null)))
            .ToImmutableDictionary(Converter.GetGenericArgument);
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "FallbackConvertersMethods");
        var field = type.GetFieldNotNull("SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ImmutableDictionary<Type, IConverter>>(field.GetValue(null));
        Assert.Equal(expected.Keys, actual.Keys);
        Assert.Equal(expected.Values.Select(x => x.GetType()).ToHashSet(), actual.Values.Select(x => x.GetType()).ToHashSet());
    }

    [Fact(DisplayName = "Generator Shared Converter Creators")]
    public void GeneratorSharedConverterCreators()
    {
        var expected = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsAbstract is false && typeof(IConverterCreator).IsAssignableFrom(x))
            .Select(x => Assert.IsAssignableFrom<IConverterCreator>(Activator.CreateInstance(x, null)))
            .ToList();
        Assert.Equal(1, expected.RemoveAll(x => x.GetType().Name is "OldConverterCreator"));
        // ensure internal converter creators
        _ = Generator.CreateDefaultBuilder();
        var field = typeof(Generator).GetFieldNotNull("SharedConverterCreators", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ConcurrentDictionary<string, IConverterCreator>>(field.GetValue(null));
        Assert.Equal(expected.Select(x => x.GetType()).ToHashSet(), actual.Values.Select(x => x.GetType()).ToHashSet());
    }

#if NET7_0_OR_GREATER
    [Fact(DisplayName = "Raw Converter Creator Shared Converters")]
    public void RawConverterCreatorSharedConverters()
    {
        var rawConverterType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "RawConverter`2");
        var expectedQuery =
            from i in typeof(IConverter).Assembly.GetTypes()
            where i.Namespace is "Mikodev.Binary.Features.Instance" && i.IsGenericType is false
            let k = i.GetInterfaces().Single().GetGenericArguments().Single()
            let t = rawConverterType.MakeGenericType(k, i)
            let v = Assert.IsAssignableFrom<IConverter>(Activator.CreateInstance(t, null))
            select KeyValuePair.Create(k, v);
        var expected = expectedQuery.ToImmutableDictionary();
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "RawConverterCreator");
        var field = type.GetFieldNotNull("SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ImmutableDictionary<Type, IConverter>>(field.GetValue(null));
        Assert.Equal(expected.Keys, actual.Keys);
        Assert.Equal(expected.Values.Select(x => x.GetType()).ToHashSet(), actual.Values.Select(x => x.GetType()).ToHashSet());
    }
#endif
}
