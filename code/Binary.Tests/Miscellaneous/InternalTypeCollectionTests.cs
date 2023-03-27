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
            .Select(x => Assert.IsAssignableFrom<IConverter>(Activator.CreateInstance(x, null)))
            .ToImmutableDictionary(Converter.GetGenericArgument);
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "IsolatedConverterCreator");
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
        // ensure internal converter creators
        _ = Generator.CreateDefaultBuilder();
        var method = typeof(Generator).GetMethodNotNull("GetConverterCreators", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<IEnumerable<IConverterCreator>>(method.Invoke(null, null)).ToList();
        Assert.Equal(expected.Select(x => x.GetType()).ToHashSet(), actual.Select(x => x.GetType()).ToHashSet());
    }

    [Fact(DisplayName = "Endianness Shared Converters")]
    public void EndiannessSharedConverters()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DetectEndianConverterCreator");
        var field = ReflectionExtensions.GetFieldNotNull(type, "SharedConverters", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ImmutableDictionary<Type, (IConverter, IConverter)>>(field.GetValue(null));
        foreach (var (key, (little, native)) in actual)
        {
            Assert.Equal(key, Converter.GetGenericArgument(little));
            Assert.Equal(key, Converter.GetGenericArgument(native));
            Assert.True(little.GetType().Name is "LittleEndianConverter`1" or "RepeatLittleEndianConverter`2");
            Assert.Equal("NativeEndianConverter`1", native.GetType().Name);
        }
    }
}
