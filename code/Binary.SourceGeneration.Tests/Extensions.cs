namespace Mikodev.Binary.SourceGeneration.Tests;

using Microsoft.CodeAnalysis;
using Xunit;

public static class Extensions
{
    public static string GetSourceText(this Location location)
    {
        var span = location.SourceSpan;
        var tree = location.SourceTree;
        Assert.NotNull(tree);
        var text = tree.GetText().GetSubText(span).ToString();
        return text;
    }
}
