namespace Mikodev.Binary.SourceGeneration.Tests.CircularTypeReferenceTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

public class CustomLinkedListTests
{
    public static IEnumerable<object[]> CustomLinkedListAsNamedObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<LinkedList<int>>]
            public partial class LinkedListGeneratorContext { }

            [NamedObject]
            public class LinkedList<T>(T data, LinkedList<T> next)
            {
                [NamedKey("data")]
                public T Data = data;

                [NamedKey("next")]
                public LinkedList<T> Next = next;
            }
            """;
        yield return [a];
    }

    public static IEnumerable<object[]> CustomLinkedListAsPlainObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<LinkedList<int>>]
            public partial class LinkedListGeneratorContext { }

            public class LinkedList<T>(T data, LinkedList<T> next)
            {
                public T Data = data;

                public LinkedList<T> Next = next;
            }
            """;
        yield return [a];
    }

    public static IEnumerable<object[]> CustomLinkedListAsTupleObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<LinkedList<string>>]
            public partial class LinkedListGeneratorContext { }

            [TupleObject]
            public class LinkedList<T>(T data, LinkedList<T> next)
            {
                [TupleKey(0)]
                public T Data = data;

                [TupleKey(1)]
                public LinkedList<T> Next = next;
            }
            """;
        yield return [a, "Next", "LinkedList<String>"];
    }

    [Theory(DisplayName = "Custom Linked List As Named Object Test")]
    [MemberData(nameof(CustomLinkedListAsNamedObjectData))]
    [MemberData(nameof(CustomLinkedListAsPlainObjectData))]
    public void CustomLinkedListAsNamedObjectTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source,
        [
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FrozenDictionary).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        ]);
        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generators: [generator.AsSourceGenerator()], parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostics = compilation.GetDiagnostics();
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        Assert.Empty(outputDiagnostics);
        Assert.Empty(outputCompilationDiagnostics);
        var sourceCode = outputCompilation.SyntaxTrees.Last().ToString();
        Assert.Contains("return (Mikodev.Binary.IConverter)converter", sourceCode);
    }

    [Theory(DisplayName = "Custom Linked List As Tuple Object Test")]
    [MemberData(nameof(CustomLinkedListAsTupleObjectData))]
    public void CustomLinkedListAsTupleObjectTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source,
        [
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FrozenDictionary).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        ]);
        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generators: [generator.AsSourceGenerator()], parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostics = compilation.GetDiagnostics();
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        var diagnostic = Assert.Single(outputDiagnostics);
        Assert.Empty(outputCompilationDiagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("Self Type Reference Found.", diagnostic.Descriptor.Title);
        var message = $"Self type reference found, member name: {memberName}, containing type: {typeName}";
        Assert.Contains(message, diagnostic.ToString());
        Assert.Equal(memberName, diagnostic.Location.GetSourceText());
    }
}
