namespace Mikodev.Binary.SourceGeneration.Tests.ImplementationTests;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

public class MultipleContextTests
{
    public static IEnumerable<object[]> MultipleContextTypeErrorData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [NamedObject]
            class EmptyNamedObject { }

            [SourceGeneratorContext]
            [SourceGeneratorInclude<EmptyNamedObject>]
            partial class AlphaGeneratorContext { }

            [SourceGeneratorContext]
            [SourceGeneratorInclude<EmptyNamedObject>]
            partial class BravoGeneratorContext { }
            """;
        yield return [a, "EmptyNamedObject", "No available member found", "NamedObject"];
    }

    [Theory(DisplayName = "Multiple Context Type Error Test")]
    [MemberData(nameof(MultipleContextTypeErrorData))]
    public void MultipleContextTypeErrorTest(string source, string typeName, string message, string location)
    {
        var contextMatches = Regex.Matches(source, @"\[SourceGeneratorContext\]");
        var includeMatches = Regex.Matches(source, $@"\[SourceGeneratorInclude<{typeName}>\]");
        Assert.Equal(2, contextMatches.Count);
        Assert.Equal(2, includeMatches.Count);
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        // only report error one time
        var diagnostic = Assert.Single(diagnostics);
        Assert.Contains(message, diagnostic.ToString());
        Assert.Equal(location, diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> MultipleContextIncludeErrorData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            class EmptyObject { }

            [SourceGeneratorContext]
            [SourceGeneratorInclude<EmptyObject>]
            partial class AlphaGeneratorContext { }

            [SourceGeneratorContext]
            [SourceGeneratorInclude<EmptyObject>]
            partial class BravoGeneratorContext { }
            """;
        yield return [a, "EmptyObject", "No available member found"];
    }

    [Theory(DisplayName = "Multiple Context Include Error Test")]
    [MemberData(nameof(MultipleContextIncludeErrorData))]
    public void MultipleContextIncludeErrorTest(string source, string typeName, string message)
    {
        var contextMatches = Regex.Matches(source, @"\[SourceGeneratorContext\]");
        var includeMatches = Regex.Matches(source, $@"\[SourceGeneratorInclude<{typeName}>\]");
        Assert.Equal(2, contextMatches.Count);
        Assert.Equal(2, includeMatches.Count);
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Equal(2, diagnostics.Length);
        var x = diagnostics[0];
        var y = diagnostics[1];
        Assert.NotEqual(x.Location, y.Location);
        Assert.Contains(message, x.ToString());
        Assert.Contains(message, y.ToString());
        Assert.Equal($"SourceGeneratorInclude<{typeName}>", x.Location.GetSourceText());
        Assert.Equal($"SourceGeneratorInclude<{typeName}>", y.Location.GetSourceText());
    }
}
