namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.DirectTests;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

public class FakeUnionTypeWithEmptyBody { }

public class FakeUnionTypeWithInvalidValuePropertyType
{
    public int Value { get; }
}

public class FakeUnionTypeWithInvalidPrivateValueProperty
{
    private object? Value { get; }
}

public class FakeUnionTypeWithInvalidIndexerValueProperty
{
    [IndexerName("Value")]
    public object? this[int index] => null;
}

public class FakeUnionTypeWithPrivateGetterValueProperty
{
    public object? Value { private get; set; }
}

public class FakeUnionTypeWithSetterOnlyValueProperty
{
    public object? Value { set { } }
}

public class FakeUnionTypeWithGoodValueProperty
{
    public object? Value { get; }
}

public class FakeUnionTypeWithGoodValuePropertyAndExtraProperty
{
    public object? Value { get; }

    public int Extra { get; }
}

public class UnionConverterCreatorInternalTests
{
    private delegate PropertyInfo SelectUnionValueProperty(Type type, ImmutableArray<Type?> types);

    [Theory(DisplayName = "Select Union Value Property With Invalid Type Test")]
    [InlineData(typeof(FakeUnionTypeWithEmptyBody))]
    [InlineData(typeof(FakeUnionTypeWithInvalidValuePropertyType))]
    [InlineData(typeof(FakeUnionTypeWithInvalidPrivateValueProperty))]
    [InlineData(typeof(FakeUnionTypeWithInvalidIndexerValueProperty))]
    [InlineData(typeof(FakeUnionTypeWithPrivateGetterValueProperty))]
    [InlineData(typeof(FakeUnionTypeWithSetterOnlyValueProperty))]
    public void SelectUnionValuePropertyWithInvalidTypeTest(Type type)
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "UnionConverterCreator");
        var select = (SelectUnionValueProperty)Delegate.CreateDelegate(typeof(SelectUnionValueProperty), Assert.IsType<MethodInfo>(creatorType.GetMethod(nameof(SelectUnionValueProperty), BindingFlags.Static | BindingFlags.NonPublic), exactMatch: false));
        Assert.NotNull(select);
        var error = Assert.Throws<ArgumentException>(() => select.Invoke(type, [type]));
        var message = $"Union value property detection failed, type: {type}";
        Assert.Equal(message, error.Message);
    }

    [Theory(DisplayName = "Select Union Value Property With Valid Type Test")]
    [InlineData(typeof(FakeUnionTypeWithGoodValueProperty))]
    [InlineData(typeof(FakeUnionTypeWithGoodValuePropertyAndExtraProperty))]
    public void SelectUnionValuePropertyWithValidTypeTest(Type type)
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "UnionConverterCreator");
        var select = (SelectUnionValueProperty)Delegate.CreateDelegate(typeof(SelectUnionValueProperty), Assert.IsType<MethodInfo>(creatorType.GetMethod(nameof(SelectUnionValueProperty), BindingFlags.Static | BindingFlags.NonPublic), exactMatch: false));
        Assert.NotNull(select);
        var property = select.Invoke(type, [type]);
        Assert.Equal("Value", property.Name);
        Assert.Equal(typeof(object), property.PropertyType);
    }

    [Fact(DisplayName = "Select Union Value Property With Null Type Test")]
    public void SelectUnionValuePropertyWithNullTypeTest()
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "UnionConverterCreator");
        var select = (SelectUnionValueProperty)Delegate.CreateDelegate(typeof(SelectUnionValueProperty), Assert.IsType<MethodInfo>(creatorType.GetMethod(nameof(SelectUnionValueProperty), BindingFlags.Static | BindingFlags.NonPublic), exactMatch: false));
        Assert.NotNull(select);
        var error = Assert.Throws<ArgumentException>(() => select.Invoke(typeof(object), [null]));
        var message = $"Union value property detection failed, type: {typeof(object)}";
        Assert.Equal(message, error.Message);
    }
}
