namespace Mikodev.Binary.SourceGeneration.Tests.ExtensionTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class SymbolExtensionTests
{
    public static IEnumerable<object[]> AttributeConstructorArgumentData()
    {
        var a =
            """
            namespace Tests;

            using System;

            class TestAttribute(int some) : Attribute { }

            [Test(1)]
            class TestClass { }
            """;
        var b =
            """
            namespace Tests;

            using System;

            class TestAttribute(string some) : Attribute { }

            [Test("data")]
            class TestClass { }
            """;
        yield return [a, 1];
        yield return [b, "data"];
    }

    [Theory(DisplayName = "Attribute Constructor Argument Test")]
    [MemberData(nameof(AttributeConstructorArgumentData))]
    public void AttributeConstructorArgumentTest<T>(string source, T expected)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<ClassDeclarationSyntax>().Last();
        var symbol = Assert.IsAssignableFrom<ITypeSymbol>(model.GetDeclaredSymbol(declaration));
        var attribute = symbol.GetAttributes().Single();
        var status = SymbolExtensions.TryGetConstructorArgument<T>(attribute, out var result);
        Assert.True(status);
        Assert.Equal(expected, result);
        var statusError = SymbolExtensions.TryGetConstructorArgument<Tuple<T>>(attribute, out var resultError);
        Assert.False(statusError);
        Assert.Null(resultError);
    }

    public static IEnumerable<object[]> AttributeConstructorArgumentInvalidCountData()
    {
        var a =
            """
            namespace Tests;

            using System;

            class TestAttribute() : Attribute { }

            [Test()]
            class TestClass { }
            """;
        var b =
            """
            namespace Tests;

            using System;

            class TestAttribute(string a, string b) : Attribute { }

            [Test("1", "2")]
            class TestClass { }
            """;
        yield return [a];
        yield return [b];
    }

    [Theory(DisplayName = "Attribute Constructor Argument Invalid Count Test")]
    [MemberData(nameof(AttributeConstructorArgumentInvalidCountData))]
    public void AttributeConstructorArgumentInvalidCountTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<ClassDeclarationSyntax>().Last();
        var symbol = Assert.IsAssignableFrom<ITypeSymbol>(model.GetDeclaredSymbol(declaration));
        var attribute = symbol.GetAttributes().Single();
        var status = SymbolExtensions.TryGetConstructorArgument<string>(attribute, out var result);
        Assert.False(status);
        Assert.Null(result);
    }
}
