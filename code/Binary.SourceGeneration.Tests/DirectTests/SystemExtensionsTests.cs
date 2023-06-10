namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Mikodev.Binary.SourceGeneration.Internal;
using System;
using System.Linq;
using System.Text;
using Xunit;

public class SystemExtensionsTests
{
    [Theory(DisplayName = "Indent Levels Invalid Argument Test")]
    [InlineData(-1)]
    [InlineData(17)]
    public void IndentLevelsInvalidArgumentTest(int indent)
    {
        var builder = new StringBuilder();
        var f = new Action<StringBuilder, int, string>(SystemExtensions.AppendIndent);
        var g = new Action<StringBuilder, int, string, string, int, Func<int, string>>(SystemExtensions.AppendIndent);
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
        SystemExtensions.AppendIndent(a, indent, string.Empty);
        SystemExtensions.AppendIndent(b, indent, string.Empty, string.Empty, 0, _ => string.Empty);
        var buffer = (stackalloc char[expectedLength]);
        buffer.Fill(' ');
        var expected = string.Concat(buffer, Environment.NewLine);
        Assert.Equal(expected, a.ToString());
        Assert.Equal(expected, b.ToString());
    }
}
