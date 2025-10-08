namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class MultipleAttributesTests
{
    public static IEnumerable<object[]> MultipleAttributesOnTypeData()
    {
        var alpha =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            [Converter(typeof(object))]
            public class Alpha { }
            """;
        var bravo =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class BravoSourceGeneratorContext { }

            [TupleObject]
            [ConverterCreator(typeof(object))]
            public class Bravo { }
            """;
        var delta =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            public partial class DeltaSourceGeneratorContext { }

            [Converter(typeof(object))]
            [ConverterCreator(typeof(object))]
            public class Delta { }
            """;
        yield return [alpha, "Alpha"];
        yield return [bravo, "Bravo"];
        yield return [delta, "Delta"];
    }

    [Theory(DisplayName = "Multiple Attributes Found On Type")]
    [MemberData(nameof(MultipleAttributesOnTypeData))]
    public void MultipleAttributesOnTypeTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics.Where(x => x.ToString().Contains("Multiple attributes")));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Multiple attributes found, type: {typeName}", diagnostic.ToString());
        Assert.Contains(typeName, diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> MultipleAttributesOnMemberData()
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
        yield return [alpha, "T01", "Tag"];
    }

    [Theory(DisplayName = "Multiple Attributes Found On Member")]
    [MemberData(nameof(MultipleAttributesOnMemberData))]
    public void MultipleAttributesOnMemberTest(string source, string typeName, string memberName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics.Where(x => x.ToString().Contains("Multiple attributes")));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Multiple attributes found, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Contains(memberName, diagnostic.Location.GetSourceText());

        // not important
        _ = Assert.Single(diagnostics.Where(x => x.ToString().Contains("Require converter type")));
        _ = Assert.Single(diagnostics.Where(x => x.ToString().Contains("Require converter creator type")));
    }
}
