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
        yield return new object[] { a, "Array", "global::System.Int32[,]", "Array2D_l_System_Int32_r" };
        yield return new object[] { b, "Value", "global::System.String[]", "Array_l_System_String_r" };
        yield return new object[] { c, "Items", "global::System.Double[,,,]", "Array4D_l_System_Double_r" };
        yield return new object[] { d, "Entry", "global::System.Int32[][]", "Array_l_Array_l_System_Int32_r_r" };
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
        yield return new object[] { a, "Item", "global::A", "g_A" };
        yield return new object[] { b, "Data", "global::B<global::System.Int32>", "g_B_l_System_Int32_r" };
    }

    public static IEnumerable<object[]> NestedTypeData()
    {
        var a =
            """
            namespace One.Two;

            class A
            {
                public class X
                {
                    public class Y { }
                }
            }

            class Alpha
            {
                public A.X.Y Item;
            }
            """;
        var b =
            """
            class B<T>
            {
                public class X
                {
                    public class Y<R> { }
                }
            }

            class Bravo
            {
                public B<int>.X.Y<string> Data;
            }
            """;
        yield return new object[] { a, "Item", "global::One.Two.A.X.Y", "One_Two_A_X_Y" };
        yield return new object[] { b, "Data", "global::B<global::System.Int32>.X.Y<global::System.String>", "g_B_l_System_Int32_r_X_Y_l_System_String_r" };
    }

    [Theory(DisplayName = "Get Full Name Test")]
    [MemberData(nameof(ArrayData))]
    [MemberData(nameof(GlobalNamespaceTypeData))]
    [MemberData(nameof(NestedTypeData))]
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
