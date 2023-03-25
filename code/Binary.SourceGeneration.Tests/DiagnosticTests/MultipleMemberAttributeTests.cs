namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class MultipleMemberAttributeTests
{
    public static IEnumerable<object[]> MultipleAttributesData()
    {
        var alpha =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<T01>]
            public partial class T01SourceGeneratorContext { }

            [NamedObject]
            public class T01
            {
                [Converter(typeof(object))]
                [ConverterCreator(typeof(object))]
                [NamedKey("1")]
                public string? Tag { get; }
            }
            """;
        yield return new object[] { alpha, "T01", "Tag" };
    }

    [Theory(DisplayName = "Multiple Attributes Found On Member")]
    [MemberData(nameof(MultipleAttributesData))]
    public void MultipleAttributesTest(string source, string typeName, string memberName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Multiple attributes found, member name: {memberName}, type: {typeName}", diagnostic.ToString());
        Assert.Contains(memberName, diagnostic.Location.GetSourceText());
    }
}
