namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

public class InternalTypeCollectionTests
{
    [Fact(DisplayName = "Isolated Converters Methods Shared Converters")]
    public void FallbackConvertersMethodsSharedConverters()
    {
        var expected = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.Namespace?.StartsWith("Mikodev.Binary.Creators.Isolated") is true && x.IsAssignableTo(typeof(IConverter)))
            .Select(x => Assert.IsType<IConverter>(Activator.CreateInstance(x, null), exactMatch: false))
            .ToImmutableDictionary(Converter.GetGenericArgument);
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "IsolatedConverterCreator");
        var field = type.GetFieldNotNull("SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsType<IReadOnlyDictionary<Type, IConverter>>(field.GetValue(null), exactMatch: false);
        Assert.Equal(expected.Keys.ToHashSet(), [.. actual.Keys]);
        Assert.Equal(expected.Values.Select(x => x.GetType()).ToHashSet(), [.. actual.Values.Select(x => x.GetType())]);
    }

    [Fact(DisplayName = "Generator Shared Converter Creators")]
    public void GeneratorSharedConverterCreators()
    {
        var expected = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsAbstract is false && typeof(IConverterCreator).IsAssignableFrom(x))
            .Select(x => Assert.IsType<IConverterCreator>(Activator.CreateInstance(x, null), exactMatch: false))
            .ToList();
        // ensure internal converter creators
        _ = Generator.CreateDefaultBuilder();
        var method = typeof(Generator).GetMethodNotNull("GetConverterCreators", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsType<IEnumerable<IConverterCreator>>(method.Invoke(null, null), exactMatch: false).ToList();
        Assert.Equal(expected.Select(x => x.GetType()).ToHashSet(), [.. actual.Select(x => x.GetType())]);
    }

    [Fact(DisplayName = "Endianness Shared Converters")]
    public void EndiannessSharedConverters()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "LittleEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsType<IReadOnlyDictionary<Type, IConverter>>(field.GetValue(null), exactMatch: false);
        foreach (var (key, converter) in actual)
        {
            Assert.Equal(key, Converter.GetGenericArgument(converter));
            Assert.True(converter.GetType().Name is "LittleEndianConverter`1" or "RepeatLittleEndianConverter`2");
        }
    }
}
