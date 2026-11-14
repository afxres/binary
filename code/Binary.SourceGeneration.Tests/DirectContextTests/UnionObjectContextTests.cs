#if NET11_0_OR_GREATER

namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

public class UnionObjectContextTests
{
    public static IEnumerable<object[]> InvalidUnionObjectData()
    {
        var a =
            """
            // union empty body
            namespace UnionTests;

            using Mikodev.Binary.Attributes;
            using System.Runtime.CompilerServices;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<FakeUnion>]
            partial class TestGeneratorContext { }

            [Union]
            readonly struct FakeUnion { }
            """;
        yield return [a];
    }

    public static IEnumerable<object[]> EmptyUnionCaseTypeData()
    {
        yield return ["readonly record struct Empty;"];
        yield return ["sealed class Empty { }"];
        yield return ["sealed record Empty;"];
    }

    private static (SourceResult.Success Success, ImmutableArray<ITypeSymbol> Tracked) InvokeUnion(string source, string unionTypeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var symbol = compilation.GetTypeByMetadataName(unionTypeName) ?? throw new InvalidOperationException(unionTypeName);
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var tracked = ImmutableArray.CreateBuilder<ITypeSymbol>();
        var context = new SourceGeneratorContext(compilation, diagnostics.Add, CancellationToken.None);
        var tracker = new SourceGeneratorTracker(tracked.Add);
        var result = UnionObjectConverterContext.Invoke(context, tracker, symbol)?.Value;
        Assert.DoesNotContain(diagnostics, x => x.Severity is DiagnosticSeverity.Error);
        return (Assert.IsType<SourceResult.Success>(result), tracked.ToImmutable());
    }

    [Theory(DisplayName = "Empty Union Case Type Test")]
    [MemberData(nameof(EmptyUnionCaseTypeData))]
    public void EmptyUnionCaseTypeTest(string declaration)
    {
        // 'record class' synthesizes 'EqualityContract', which is not state: it must not request a converter
        var source =
            $$"""
            namespace UnionTests;

            {{declaration}}

            readonly union Sample(Empty, int);
            """;
        var (success, tracked) = InvokeUnion(source, "UnionTests.Sample");
        var trackedType = Assert.Single(tracked);
        Assert.Equal(SpecialType.System_Int32, trackedType.SpecialType);
        Assert.Contains("case global::UnionTests.Empty:", success.SourceCode);
        Assert.Contains("=> new global::UnionTests.Empty(),", success.SourceCode);
        Assert.DoesNotContain("GetConverter<global::UnionTests.Empty>", success.SourceCode);
    }

    [Fact(DisplayName = "Non Empty Union Case Type Test")]
    public void NonEmptyUnionCaseTypeTest()
    {
        var source =
            """
            namespace UnionTests;

            sealed record Payload(int Value);

            readonly union Sample(Payload, int);
            """;
        var (success, tracked) = InvokeUnion(source, "UnionTests.Sample");
        Assert.Equal(2, tracked.Length);
        Assert.Contains("GetConverter<global::UnionTests.Payload>", success.SourceCode);
    }

    [Fact(DisplayName = "Private State Union Case Type Test")]
    public void PrivateStateUnionCaseTypeTest()
    {
        // non public members count as state, otherwise the private field would be dropped silently
        var source =
            """
            namespace UnionTests;

            readonly struct Hidden
            {
                private readonly int value;

                public Hidden(int value) => this.value = value;

                public int Read() => this.value;
            }

            readonly union Sample(Hidden, int);
            """;
        var (success, tracked) = InvokeUnion(source, "UnionTests.Sample");
        Assert.Equal(2, tracked.Length);
        Assert.Contains("GetConverter<global::UnionTests.Hidden>", success.SourceCode);
    }

    [Fact(DisplayName = "Value Tuple Union Case Type Test")]
    public void ValueTupleUnionCaseTypeTest()
    {
        var source =
            """
            namespace UnionTests;

            readonly union Sample(System.ValueTuple, int);
            """;
        var (success, tracked) = InvokeUnion(source, "UnionTests.Sample");
        var trackedType = Assert.Single(tracked);
        Assert.Equal(SpecialType.System_Int32, trackedType.SpecialType);
        Assert.DoesNotContain("GetConverter<global::System.ValueTuple>", success.SourceCode);
    }

    [Fact(DisplayName = "Inherited Union Case Factory Test")]
    public void InheritedUnionCaseFactoryTest()
    {
        // a factory inherited from a base interface is not a member of the provider itself,
        // so it would be dropped silently and the case would end up unreachable at runtime
        var source =
            """
            namespace UnionTests;

            using System.Runtime.CompilerServices;

            interface IIntCaseFactory
            {
                static abstract Sample Create(int value);
            }

            [Union]
            sealed class Sample : Sample.IUnionMembers, IIntCaseFactory
            {
                private readonly object? value;

                private Sample(object? value) => this.value = value;

                public interface IUnionMembers : IIntCaseFactory
                {
                    static Sample Create(string value) => new(value);

                    object? Value { get; }
                }

                static Sample IIntCaseFactory.Create(int value) => new(value);

                object? IUnionMembers.Value => this.value;
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var symbol = compilation.GetTypeByMetadataName("UnionTests.Sample") ?? throw new InvalidOperationException("UnionTests.Sample");
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = Assert.IsType<SourceResult.Diagnostic>(UnionObjectConverterContext.Invoke(context, tracker, symbol)?.Value);
        var (descriptor, messageArguments) = Assert.Single(result.DiagnosticArguments);
        Assert.Equal(Diagnostics.UnionCaseFactoryInherited, descriptor);
        Assert.Equal(Symbols.GetSymbolDiagnosticDisplayString(symbol), Assert.Single(messageArguments!));
    }

    public static IEnumerable<object[]> UnionCaseFactoryData()
    {
        // a base interface without any case factory
        var a =
            """
            namespace UnionTests;

            using System.Runtime.CompilerServices;

            interface IMarker { }

            [Union]
            sealed class Sample : Sample.IUnionMembers, IMarker
            {
                private readonly object? value;

                private Sample(object? value) => this.value = value;

                public interface IUnionMembers : IMarker
                {
                    static Sample Create(int value) => new(value);

                    object? Value { get; }
                }

                object? IUnionMembers.Value => this.value;
            }
            """;
        // the case factory declared by the provider itself
        var b =
            """
            namespace UnionTests;

            using System.Runtime.CompilerServices;

            [Union]
            sealed class Sample : Sample.IUnionMembers
            {
                private readonly object? value;

                private Sample(object? value) => this.value = value;

                public interface IUnionMembers
                {
                    static Sample Create(int value) => new(value);

                    object? Value { get; }
                }

                object? IUnionMembers.Value => this.value;
            }
            """;
        yield return [a];
        yield return [b];
    }

    [Theory(DisplayName = "Union Case Factory Test")]
    [MemberData(nameof(UnionCaseFactoryData))]
    public void UnionCaseFactoryTest(string source)
    {
        var (success, tracked) = InvokeUnion(source, "UnionTests.Sample");
        var trackedType = Assert.Single(tracked);
        Assert.Equal(SpecialType.System_Int32, trackedType.SpecialType);
        Assert.Contains("GetConverter<global::System.Int32>", success.SourceCode);
    }

    [Theory(DisplayName = "Ambiguous Union Case Type Test")]
    [InlineData("readonly union Sample(int?, int);")]
    [InlineData("readonly union Sample(long, string?, long?);")]
    public void AmbiguousUnionCaseTypeTest(string declaration)
    {
        // a nullable case type and its underlying case type share one boxed representation,
        // so a pattern cannot tell them apart and the generated switch would be unreachable
        var source =
            $$"""
            namespace UnionTests;

            {{declaration}}
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var symbol = compilation.GetTypeByMetadataName("UnionTests.Sample") ?? throw new InvalidOperationException("UnionTests.Sample");
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = Assert.IsType<SourceResult.Diagnostic>(UnionObjectConverterContext.Invoke(context, tracker, symbol)?.Value);
        var (descriptor, messageArguments) = Assert.Single(result.DiagnosticArguments);
        Assert.Equal(Diagnostics.UnionCaseTypeAmbiguous, descriptor);
        Assert.Equal(Symbols.GetSymbolDiagnosticDisplayString(symbol), Assert.Single(messageArguments!));
    }

    [Theory(DisplayName = "Invalid Union Object Test")]
    [MemberData(nameof(InvalidUnionObjectData))]
    public void InvalidUnionObjectTest(string source)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location)
        };
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, CompilationModule.ParseOptions)],
            references: [.. references],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var driver = CSharpGeneratorDriver.Create(generators: [new SourceGenerator().AsSourceGenerator()], parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostic = Assert.Single(outputDiagnostics);
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.DoesNotContain(outputCompilationDiagnostics, x => x.Id is "CS0234");

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<StructDeclarationSyntax>().Last();
        var symbol = Assert.IsType<ITypeSymbol>(model.GetDeclaredSymbol(declaration), exactMatch: false);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = Assert.IsType<SourceResult.Diagnostic>(UnionObjectConverterContext.Invoke(context, tracker, symbol)?.Value);
        var (descriptor, messageArguments) = Assert.Single(result.DiagnosticArguments);
        Assert.NotNull(messageArguments);
        Assert.Equal(2, messageArguments.Length);
        Assert.Equal(Diagnostics.TypeNotRecognized, descriptor);
        Assert.Equal("UnionObject", messageArguments[0]);
        Assert.Equal(Symbols.GetSymbolDiagnosticDisplayString(symbol), messageArguments[1]);
    }
}

#endif
