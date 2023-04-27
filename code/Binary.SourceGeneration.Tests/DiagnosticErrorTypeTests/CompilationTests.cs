namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticErrorTypeTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

public class CompilationTests
{
    public static IEnumerable<object[]> IncludeStaticTypeData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<StaticType>]
            partial class TestGeneratorContext { }

            static class StaticType { }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class TestGeneratorContext { }

            [NamedObject]
            class Alpha
            {
                [NamedKey("id")]
                public int Id;

                [NamedKey("error")]
                public StaticType StaticType;
            }

            static class StaticType { }
            """;
        yield return new object[] { a, "CS0718" };
        yield return new object[] { b, "CS0723" };
    }

    public static IEnumerable<object[]> IncludeNotExistTypeData()
    {
        var a =
            """
            namespace Tests;
            
            using Mikodev.Binary.Attributes;
            
            [SourceGeneratorContext]
            [SourceGeneratorInclude<NotExistType>]
            partial class TestGeneratorContext { }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class TestGeneratorContext { }

            [TupleObject]
            class Alpha
            {
                [TupleKey(0)]
                public int Id;

                [TupleKey(1)]
                public NotExistType NotExistType;
            }
            """;
        yield return new object[] { a, "CS0246" };
        yield return new object[] { b, "CS0246" };
    }

    [Theory(DisplayName = "Include Error Type Test")]
    [MemberData(nameof(IncludeStaticTypeData))]
    [MemberData(nameof(IncludeNotExistTypeData))]
    public void IncludeErrorTypeTest(string source, string diagnosticId)
    {
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();
        builder.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location));
        const string AssemblyName = "TestAssembly";
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, CompilationModule.ParseOptions) },
            references: builder.ToArray(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var originDiagnostics = compilation.GetDiagnostics();
        Assert.Equal(diagnosticId, Assert.Single(originDiagnostics.Where(x => x.Severity is DiagnosticSeverity.Error)).Id);

        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generators: new[] { generator.AsSourceGenerator() }, parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Equal(diagnosticId, Assert.Single(outputCompilationDiagnostics.Where(x => x.Severity is DiagnosticSeverity.Error)).Id);

        var diagnostic = Assert.Single(outputDiagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("Require supported type", diagnostic.ToString());
    }
}
