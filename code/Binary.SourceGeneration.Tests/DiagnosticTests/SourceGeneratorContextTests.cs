namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using Xunit;

public class SourceGeneratorContextTests
{
    [Fact(DisplayName = "Source Generator Context Type Not Partial Type")]
    public void ContextNotPartial()
    {
        var source =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            public class TestSourceGeneratorContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Require 'partial' keyword for source generator context, type: TestSourceGeneratorContext", diagnostic.ToString());
        Assert.Contains("TestSourceGeneratorContext", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "Source Generator Context Type Not In Namespace")]
    public void ContextNotInNamespace()
    {
        var source =
            """
            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            public partial class AnotherTestSourceGeneratorContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Require not global namespace for source generator context, type: AnotherTestSourceGeneratorContext", diagnostic.ToString());
        Assert.Contains("AnotherTestSourceGeneratorContext", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "Source Generator Context Type Is Nested")]
    public void ContextIsNested()
    {
        var source =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            class Outer
            {
                [SourceGeneratorContext]
                partial class NestedSourceGeneratorContext { }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Require not nested type for source generator context, type: Outer.NestedSourceGeneratorContext", diagnostic.ToString());
        Assert.Contains("NestedSourceGeneratorContext", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "Source Generator Context Type Is Generic")]
    public void ContextIsGeneric()
    {
        var source =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            partial class GenericSourceGeneratorContext<T> { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Require not generic type for source generator context, type: GenericSourceGeneratorContext<T>", diagnostic.ToString());
        Assert.Contains("GenericSourceGeneratorContext", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "Type Inclusion Duplicated")]
    public void InclusionDuplicated()
    {
        var source =
            """
            namespace OuterNamespace;
            
            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;
            
            [SourceGeneratorContext]
            [SourceGeneratorInclude<List<int>>]
            [SourceGeneratorInclude<List<int>>]
            public partial class ThirdTestSourceGeneratorContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith("Type inclusion duplicated, type: List<Int32>", diagnostic.ToString());
        Assert.Contains("SourceGeneratorInclude<List<int>>", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "No Converter Generated")]
    public void NoConverterGenerated()
    {
        var source =
            """
            namespace SomeTest;
            
            using Mikodev.Binary.Attributes;
            
            [SourceGeneratorContext]
            [SourceGeneratorInclude<string>]
            public partial class WhateverContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith("No converter generated, type: String", diagnostic.ToString());
        Assert.Contains("SourceGeneratorInclude<string>", diagnostic.Location.GetSourceText());
    }

    [Fact(DisplayName = "No Converter Generated (explicitly ignored types)")]
    public void NoConverterGeneratedExplicitlyIgnored()
    {
        var source =
            """
            namespace SomeTest;
            
            using Mikodev.Binary.Attributes;
            
            [SourceGeneratorContext]
            [SourceGeneratorInclude<int>]
            [SourceGeneratorInclude<string>]
            [SourceGeneratorInclude<object>]
            [SourceGeneratorInclude<System.Delegate>]
            [SourceGeneratorInclude<Mikodev.Binary.Token>]
            [SourceGeneratorInclude<Mikodev.Binary.IConverter>]
            [SourceGeneratorInclude<Mikodev.Binary.Converter<int>>]
            [SourceGeneratorInclude<System.Collections.IEnumerable>]
            [SourceGeneratorInclude<System.Collections.ArrayList>]
            [SourceGeneratorInclude<System.Collections.BitArray>]
            public partial class WhateverContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Equal(10, diagnostics.Length);
        foreach (var diagnostic in diagnostics)
        {
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Contains("No converter generated", diagnostic.ToString());
            Assert.Contains("SourceGeneratorInclude", diagnostic.Location.GetSourceText());
        }
    }
}
