namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Xunit;

public class SymbolsTests
{
    [Fact(DisplayName = "Get Location For Attribute Returns None Test")]
    public void GetLocationForAttributeReturnsNone()
    {
        var result = Symbols.GetLocation((AttributeData?)null);
        Assert.True(result.Kind is LocationKind.None);
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

    public static IEnumerable<object[]> GetConstructorByArgumentNotFoundData()
    {
        var a =
            """
            abstract class Alpha
            {
                public Alpha() { }
            }
            """;
        var b =
            """
            interface IAlpha { }
            """;
        var c =
            """
            class Bravo
            {
                private Bravo() { }

                internal Bravo(Bravo a) { }

                public Bravo(int a) { }

                public Bravo(int a, int b) { }
            }
            """;
        yield return new object[] { a, "Alpha" };
        yield return new object[] { b, "IAlpha" };
        yield return new object[] { c, "Bravo" };
    }

    [Theory(DisplayName = "Get Constructor By Argument Not Found Test")]
    [MemberData(nameof(GetConstructorByArgumentNotFoundData))]
    public void GetConstructorByArgumentNotFoundTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);

        var a = Symbols.GetConstructor(symbol, symbol);
        var b = Symbols.GetConstructor(compilation.CreateArrayTypeSymbol(symbol, 1), symbol);
        Assert.Null(a);
        Assert.Null(b);
    }

    public static IEnumerable<object[]> GetConstructorByArgumentData()
    {
        var a =
            """
            class Delta
            {
                private Delta() { }

                internal Delta(string a) { }

                public Delta(int a) { }

                public Delta(Delta another) { }

                public Delta(int a, int b) { }
            }
            """;
        var b =
            """
            struct Hotel
            {
                public Hotel(Hotel structure) { }
            }
            """;
        yield return new object[] { a, "Delta", "another" };
        yield return new object[] { b, "Hotel", "structure" };
    }

    [Theory(DisplayName = "Get Constructor By Argument Test")]
    [MemberData(nameof(GetConstructorByArgumentData))]
    public void GetConstructorByArgumentTest(string source, string typeName, string parameterName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);

        var constructor = Symbols.GetConstructor(symbol, symbol);
        Assert.NotNull(constructor);
        Assert.Equal(MethodKind.Constructor, constructor.MethodKind);
        Assert.Equal(Accessibility.Public, constructor.DeclaredAccessibility);
        var parameter = Assert.Single(constructor.Parameters);
        Assert.Equal(parameterName, parameter.Name);
    }

    public static IEnumerable<object[]> GetConstructorByMemberData()
    {
        var a =
            """
            class Alpha
            {
                public int Id { get; set; }
            }
            """;
        var b =
            """
            class Bravo
            {
                public string Name { get; }

                public Bravo(string name)
                {
                    Name = name;
                }
            }
            """;
        var c =
            """
            class Delta
            {
                public int Id { get; }

                public string Tag { get; set; }

                public Delta(int id)
                {
                    Id = id;
                }
            }
            """;
        var d =
            """
            class Hotel
            {
                public int A { get; }

                public int B { get; set; }

                public int C { get; set; }

                public Hotel(int b, int c)
                {
                    B = b;
                    C = c;
                }

                public Hotel(int a)
                {
                    A = a;
                }
            }
            """;
        yield return new object[] { a, "Alpha", new string[] { "Id" } };
        yield return new object[] { b, "Bravo", new string[] { "Name" } };
        yield return new object[] { c, "Delta", new string[] { "Id", "Tag" } };
        yield return new object[] { d, "Hotel", new string[] { "A", "B", "C" } };
    }

    public static IEnumerable<object[]> GetConstructorByMemberWithRequiredData()
    {
        var a =
            """
            class Alpha
            {
                public required int A { get; set; }

                public required int B { get; set; }
            }
            """;
        var b =
            """
            using System.Diagnostics.CodeAnalysis;

            struct Bravo
            {
                public required int A { get; internal set; }

                public required int B { get; internal set; }

                [SetsRequiredMembers]
                public Bravo(int a, int b) { }
            }
            """;
        var c =
            """
            using System.Diagnostics.CodeAnalysis;

            class Delta
            {
                public required int X { get; set; }

                public required int Y { get; set; }

                public required int Z { get; set; }

                [SetsRequiredMembers]
                public Delta() { }
            }
            """;
        yield return new object[] { a, "Alpha", new string[] { "A", "B" } };
        yield return new object[] { b, "Bravo", new string[] { "A", "B" } };
        yield return new object[] { c, "Delta", new string[] { "X" } };
        yield return new object[] { c, "Delta", new string[] { "X", "Y" } };
        yield return new object[] { c, "Delta", new string[] { "X", "Y", "Z" } };
    }

    [Theory(DisplayName = "Get Constructor By Member")]
    [MemberData(nameof(GetConstructorByMemberData))]
    [MemberData(nameof(GetConstructorByMemberWithRequiredData))]
    public void GetConstructorByMemberTest(string source, string typeName, string[] memberNames)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes().ToList();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);
        var memberSymbols = nodes.OfType<MemberDeclarationSyntax>()
            .Select(x => model.GetDeclaredSymbol(x))
            .OfType<IPropertySymbol>()
            .Where(x => memberNames.Contains(x.Name))
            .ToList();
        var members = memberSymbols.Select(x => new SymbolTupleMemberInfo(x)).ToImmutableArray();
        Assert.NotEmpty(members);

        var context = new SourceGeneratorContext(compilation, new Queue<ITypeSymbol>(), CancellationToken.None);
        var constructor = Symbols.GetConstructor(context, symbol, members);
        Assert.NotNull(constructor);
    }

    public static IEnumerable<object[]> GetConstructorByMemberNotFoundData()
    {
        var a =
            """
            abstract class Alpha
            {
                public int Id { get; set; }

                public Alpha(int id)
                {
                    Id = id;
                }
            }
            """;
        var b =
            """
            interface IAlpha
            {
                string Name { get; }
            }
            """;
        yield return new object[] { a, "Alpha", new string[] { "Id" } };
        yield return new object[] { b, "IAlpha", new string[] { "Name" } };
    }

    public static IEnumerable<object[]> GetConstructorByMemberWithRequiredNotFoundData()
    {
        var a =
            """
            class Alpha
            {
                public required int A { get; set; }

                public required int B { get; set; }

                public required int C { get; set; }

                public required int D { get; set; }
            }
            """;
        var b =
            """
            class Bravo
            {
                public required int A { get; set; }

                public required int B { get; set; }

                public Bravo(int a, int b)
                {
                    A = a;
                    B = b;
                }
            }
            """;
        yield return new object[] { a, "Alpha", new string[] { "A" } };
        yield return new object[] { a, "Alpha", new string[] { "A", "B" } };
        yield return new object[] { a, "Alpha", new string[] { "A", "B", "C" } };
        yield return new object[] { b, "Bravo", new string[] { "A", "B" } };
    }

    [Theory(DisplayName = "Get Constructor By Member Not Found Test")]
    [MemberData(nameof(GetConstructorByMemberNotFoundData))]
    [MemberData(nameof(GetConstructorByMemberWithRequiredNotFoundData))]
    public void GetConstructorByMemberNotFoundTest(string source, string typeName, string[] memberNames)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);
        var memberSymbols = nodes.OfType<MemberDeclarationSyntax>()
            .Select(x => model.GetDeclaredSymbol(x))
            .OfType<IPropertySymbol>()
            .Where(x => memberNames.Contains(x.Name))
            .ToList();
        var members = memberSymbols.Select(x => new SymbolTupleMemberInfo(x)).ToImmutableArray();
        Assert.NotEmpty(members);

        var context = new SourceGeneratorContext(compilation, new Queue<ITypeSymbol>(), CancellationToken.None);
        var a = Symbols.GetConstructor(context, symbol, members);
        var b = Symbols.GetConstructor(context, compilation.CreateArrayTypeSymbol(symbol, 1), members);
        Assert.Null(a);
        Assert.Null(b);
    }
}
