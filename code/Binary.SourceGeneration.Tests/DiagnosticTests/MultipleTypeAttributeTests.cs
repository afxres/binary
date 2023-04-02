namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class MultipleTypeAttributeTests
{
    public static IEnumerable<object[]> MultipleAttributesData()
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
        yield return new object[] { alpha, "Alpha" };
        yield return new object[] { bravo, "Bravo" };
        yield return new object[] { delta, "Delta" };
    }

    [Theory(DisplayName = "Multiple Attributes Found On Type")]
    [MemberData(nameof(MultipleAttributesData))]
    public void MultipleAttributesTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics.Where(x => x.ToString().Contains("Multiple attributes")));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Multiple attributes found, type: {typeName}", diagnostic.ToString());
        Assert.Contains(typeName, diagnostic.Location.GetSourceText());
    }
}
