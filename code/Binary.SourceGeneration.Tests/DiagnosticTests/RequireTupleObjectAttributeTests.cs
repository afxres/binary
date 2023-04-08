namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequireTupleObjectAttributeTests
{
    public static IEnumerable<object[]> ExplicitData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Delta
            {
                [TupleKey(1)]
                public Guid Entry;

                [NamedKey("valid")]
                public string Bucket { get; set; }
            }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Hotel>]
            public partial class TestGeneratorContext { }

            public sealed class TestConverterCreator : IConverterCreator
            {
                public IConverter? GetConverter(IGeneratorContext context, Type type) => null;
            }

            [ConverterCreator(typeof(TestConverterCreator))]
            public class Hotel
            {
                [TupleKey(3)]
                public long Value;
            }
            """;
        yield return new object[] { a, "Entry", "Delta" };
        yield return new object[] { b, "Value", "Hotel" };
    }

    [Theory(DisplayName = "Require 'TupleObjectAttribute' Test")]
    [MemberData(nameof(ExplicitData))]
    public void RequireTupleObjectAttributeTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith($"Require 'TupleObjectAttribute' for 'TupleKeyAttribute', this attribute will be ignored, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Matches(@"TupleKey\(.*\)", diagnostic.Location.GetSourceText());
    }
}
