namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

public class SymbolsTests
{
    [Fact(DisplayName = "Get Location For Attribute Returns None Test")]
    public void GetLocationForAttributeReturnsNone()
    {
        var result = Symbols.GetLocation((AttributeData?)null);
        Assert.True(result.Kind is LocationKind.None);
    }

    [Fact(DisplayName = "Get Constructor Invalid Symbol Type Test")]
    public void GetConstructorInvalidSymbolTypeTest()
    {
        var source =
            """
            class Alpha
            {
                public int[] Array;
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<ClassDeclarationSyntax>().Last();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        var memberType = Assert.IsAssignableFrom<IFieldSymbol>(symbol.GetMembers().Single(x => x.Name is "Array")).Type;
        var result = Symbols.GetConstructor<SymbolNamedMemberInfo>(memberType, ImmutableArray.Create<SymbolNamedMemberInfo>());
        Assert.Null(result);
    }

    public static IEnumerable<object[]> ArrayData()
    {
        var a =
            """
            class Alpha
            {
                public int[,] Array;
            }
            """;
        var b =
            """
            class Bravo
            {
                public string[] Value;
            }
            """;
        var c =
            """
            class Delta
            {
                public double[,,,] Items;
            }
            """;
        var d =
            """
            class Hotel
            {
                public int[][] Entry;
            }
            """;
        yield return new object[] { a, "Array", "global::System.Int32[,]", "System_Array2D_l_System_Int32_r" };
        yield return new object[] { b, "Value", "global::System.String[]", "System_Array_l_System_String_r" };
        yield return new object[] { c, "Items", "global::System.Double[,,,]", "System_Array4D_l_System_Double_r" };
        yield return new object[] { d, "Entry", "global::System.Int32[][]", "System_Array_l_System_Array_l_System_Int32_r_r" };
    }

    public static IEnumerable<object[]> GlobalNamespaceTypeData()
    {
        var a =
            """
            class A { }

            class Alpha
            {
                public A Item;
            }
            """;
        var b =
            """
            class B<T> { }

            class Bravo
            {
                public B<int> Data;
            }
            """;
        yield return new object[] { a, "Item", "global::A", "A" };
        yield return new object[] { b, "Data", "global::B<global::System.Int32>", "B_l_System_Int32_r" };
    }

    [Theory(DisplayName = "Get Full Name Test")]
    [MemberData(nameof(ArrayData))]
    [MemberData(nameof(GlobalNamespaceTypeData))]
    public void GetFullNameTest(string source, string memberName, string symbolFullName, string outputFullName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<ClassDeclarationSyntax>().Last();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        var members = symbol.GetMembers();
        var member = members.Single(x => x.Name == memberName);
        var memberType = ((IFieldSymbol)member).Type;
        var a = Symbols.GetSymbolFullName(memberType);
        var b = Symbols.GetOutputFullName(memberType);
        Assert.Equal(symbolFullName, a);
        Assert.Equal(outputFullName, b);
    }
}
