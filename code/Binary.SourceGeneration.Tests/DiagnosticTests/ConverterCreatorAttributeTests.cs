namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class ConverterCreatorAttributeTests
{
    public static IEnumerable<object[]> TypeNullOrInvalidConverterData()
    {
        var alpha =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<XA>]
            public partial class XASourceGeneratorContext { }

            [ConverterCreator(null)]
            public class XA { }
            """;
        var bravo =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<XB>]
            public partial class XBSourceGeneratorContext { }

            [ConverterCreator(typeof(object))]
            public class XB { }
            """;
        yield return new object[] { alpha, "ConverterCreator(null)" };
        yield return new object[] { bravo, "ConverterCreator(typeof(object))" };
    }

    public static IEnumerable<object[]> MemberNullOrInvalidConverterData()
    {
        var alpha =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<YA>]
            public partial class YASourceGeneratorContext { }

            [NamedObject]
            public class YA
            {
                [NamedKey("id")]
                [ConverterCreator(null)]
                public int Id { get; set; }
            }
            """;
        var bravo =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<YB>]
            public partial class YBSourceGeneratorContext { }

            [NamedObject]
            public class YB
            {
                [NamedKey("id")]
                [ConverterCreator(typeof(string))]
                public string? Name { get; set; }
            }
            """;
        yield return new object[] { alpha, "ConverterCreator(null)" };
        yield return new object[] { bravo, "ConverterCreator(typeof(string))" };
    }

    [Theory(DisplayName = "Null Or Invalid Converter Test")]
    [MemberData(nameof(TypeNullOrInvalidConverterData))]
    [MemberData(nameof(MemberNullOrInvalidConverterData))]
    public void NullOrInvalidConverterData(string source, string location)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require converter creator type.", diagnostic.ToString());
        Assert.Contains(location, diagnostic.Location.GetSourceText());
    }
}
