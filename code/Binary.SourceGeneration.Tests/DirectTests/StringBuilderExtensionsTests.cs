namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Mikodev.Binary.SourceGeneration.Internal;
using System;
using System.Linq;
using System.Text;
using Xunit;

public class StringBuilderExtensionsTests
{
    [Theory(DisplayName = "Indent Levels Invalid Argument Test")]
    [InlineData(-1)]
    [InlineData(17)]
    public void IndentLevelsInvalidArgumentTest(int indent)
    {
        var builder = new StringBuilder();
        var f = new Action<StringBuilder, int, string>(StringBuilderExtensions.AppendIndent);
        var g = new Action<StringBuilder, int, string, string, int, Func<int, string>>(StringBuilderExtensions.AppendIndent);
        var a = Assert.Throws<ArgumentOutOfRangeException>(() => f.Invoke(builder, indent, string.Empty));
        var b = Assert.Throws<ArgumentOutOfRangeException>(() => g.Invoke(builder, indent, string.Empty, string.Empty, 0, _ => string.Empty));
        Assert.NotNull(a.ParamName);
        Assert.NotNull(b.ParamName);
        Assert.Contains(a.ParamName, f.Method.GetParameters().Select(x => x.Name));
        Assert.Contains(b.ParamName, g.Method.GetParameters().Select(x => x.Name));
    }

    [Theory(DisplayName = "Indent Test")]
    [InlineData(0, 0)]
    [InlineData(1, 4)]
    [InlineData(8, 32)]
    public void IndentTest(int indent, int expectedLength)
    {
        var a = new StringBuilder();
        var b = new StringBuilder();
        StringBuilderExtensions.AppendIndent(a, indent, string.Empty);
        StringBuilderExtensions.AppendIndent(b, indent, string.Empty, string.Empty, 0, _ => string.Empty);
        var buffer = (stackalloc char[expectedLength]);
        buffer.Fill(' ');
        var expected = string.Concat(buffer, Environment.NewLine);
        Assert.Equal(expected, a.ToString());
        Assert.Equal(expected, b.ToString());
    }

    [Theory(DisplayName = "Indent With Filter Test")]
    [InlineData(new[] { true, true, true }, "v0, v1, v2")]
    [InlineData(new[] { true, false, true }, "v0, v2")]
    [InlineData(new[] { true, true, false }, "v0, v1")]
    [InlineData(new[] { false, true, true }, "v1, v2")]
    [InlineData(new[] { false, false, false }, "")]
    public void IndentWithFilterTest(bool[] filter, string expected)
    {
        // the separator belongs between the emitted parts, not between the indexes,
        // so a skipped last index must not leave a trailing separator behind
        var builder = new StringBuilder();
        StringBuilderExtensions.AppendIndent(builder, 0, "[", "]", filter.Length, i => filter[i], i => $"v{i}");
        Assert.Equal($"[{expected}]{Environment.NewLine}", builder.ToString());
    }
}
