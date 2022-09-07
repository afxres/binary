namespace Mikodev.Binary.Tests.Legacies;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Net;
using System.Reflection;
using Xunit;

public class ConverterCreatorTests
{
    [Fact(DisplayName = "Get Converter Return Null")]
    public void GetConverterNotFound()
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("OldConverterCreator");
        Assert.Null(creator.GetConverter(null!, typeof(Uri)));
        Assert.Null(creator.GetConverter(null!, typeof(string)));
        Assert.Null(creator.GetConverter(null!, typeof(IPEndPoint)));
    }

    [Fact(DisplayName = "Collection Adapter Type")]
    public void CollectionAdapterType()
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("OldConverterCreator");
        var converter = creator.GetConverter(null!, typeof(int));
        Assert.NotNull(converter);
        var generator = Generator.CreateDefaultBuilder().AddConverter(converter).Build();
        var arrayConverter = generator.GetConverter<int[]>();
        var field = ReflectionExtensions.GetFieldNotNull(arrayConverter.GetType(), "invoke", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapter = field.GetValue(arrayConverter);
        Assert.NotNull(adapter);
        Assert.Equal("NativeEndianSequenceAdapter`1", adapter.GetType().Name);
    }
}
