namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Tests.Internal;
using System;
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
            .Where(x => x.Namespace is "Mikodev.Binary.Creators" && typeof(IConverterCreator).IsAssignableFrom(x))
            .Select(x => Assert.IsAssignableFrom<IConverterCreator>(Activator.CreateInstance(x, null)))
            .ToImmutableArray();
        var field = typeof(Generator).GetFieldNotNull("SharedConverterCreators", BindingFlags.Static | BindingFlags.NonPublic);
        var actual = Assert.IsAssignableFrom<ImmutableArray<IConverterCreator>>(field.GetValue(null));
        Assert.Equal(expected.Select(x => x.GetType()).ToHashSet(), actual.Select(x => x.GetType()).ToHashSet());
    }
}
