namespace Mikodev.Binary.Features.Tests;

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class IntegrationTests
{
    public static IEnumerable<object[]> SimpleObjectData => new List<object[]>
    {
        new object[] { 0 },
        new object[] { 2.3 },
        new object[] { DateOnly.Parse("2001-02-03") },
        new object[] { DateTimeOffset.Parse("2020-02-02T11:22:33+04:00") },
        new object[] { DateTime.Parse("2001-02-03T04:05:06") },
        new object[] { decimal.Parse("2.71828") },
        new object[] { Guid.Parse("f28a5581-c80d-4d66-84cf-790d48e877d1") },
        new object[] { (Rune)'#' },
        new object[] { TimeOnly.Parse("12:34:56") },
        new object[] { TimeSpan.Parse("01:23:45.6789") },
    };

    [Theory(DisplayName = "Simple Object")]
    [MemberData(nameof(SimpleObjectData))]
    public void SimpleObject(object data)
    {
        var generator = Generator
            .CreateDefaultBuilder()
            .AddPreviewFeaturesConverterCreators()
            .Build();
        var converter = generator.GetConverter(data.GetType());
        Assert.StartsWith("RawConverter`2", converter.GetType().Name);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }
}
