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
        yield return new object[] { a, "Array", "global::System.Int32[,]", "_Z7Array2DIN6System5Int32EE" };
        yield return new object[] { b, "Value", "global::System.String[]", "_Z5ArrayIN6System6StringEE" };
        yield return new object[] { c, "Items", "global::System.Double[,,,]", "_Z7Array4DIN6System6DoubleEE" };
        yield return new object[] { d, "Entry", "global::System.Int32[][]", "_Z5ArrayI5ArrayIN6System5Int32EEE" };
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
        yield return new object[] { a, "Item", "global::A", "_Z1A" };
        yield return new object[] { b, "Data", "global::B<global::System.Int32>", "_Z1BIN6System5Int32EE" };
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
        yield return new object[] { a, "Item", "global::One.Two.A.X.Y", "_ZN3One3Two1A1X1YE" };
        yield return new object[] { b, "Data", "global::B<global::System.Int32>.X.Y<global::System.String>", "_ZN1BIN6System5Int32EE1X1YIN6System6StringEEE" };
    }

    public static IEnumerable<object[]> SpecialNameData()
    {
        var a =
            """
            namespace @class.@yield.@namespace.@async.@await;

            class @public
            {
                public class @class { }
            }

            class Ignore
            {
                public @public Alpha;

                public @public.@class Bravo;
            }
            """;
        yield return new object[] { a, "Alpha", "global::@class.@yield.@namespace.@async.@await.@public", "_ZN5class5yield9namespace5async5await6publicE" };
        yield return new object[] { a, "Bravo", "global::@class.@yield.@namespace.@async.@await.@public.@class", "_ZN5class5yield9namespace5async5await6public5classE" };
    }

    public static IEnumerable<object[]> NestedMultipleTypeArgumentsGenericTypeData()
    {
        var source =
            """
            namespace Alpha.Bravo;

            class Generic<T, U>
            {
                public class NestedGeneric<X, Y, Z> { }
            }

            class Alpha
            {
                public Generic<int, string>.NestedGeneric<int, string, double> Intent;
            }
            """;
        var symbolFullName = "global::Alpha.Bravo.Generic<global::System.Int32, global::System.String>.NestedGeneric<global::System.Int32, global::System.String, global::System.Double>";
        var outputFullName = "_ZN5Alpha5Bravo7GenericIN6System5Int32EN6System6StringEE13NestedGenericIN6System5Int32EN6System6StringEN6System6DoubleEEE";
        yield return new object[] { source, "Intent", symbolFullName, outputFullName };
    }

    [Theory(DisplayName = "Get Full Name Test")]
    [MemberData(nameof(ArrayData))]
    [MemberData(nameof(GlobalNamespaceTypeData))]
    [MemberData(nameof(NestedTypeData))]
    [MemberData(nameof(SpecialNameData))]
    [MemberData(nameof(NestedMultipleTypeArgumentsGenericTypeData))]
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

        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var typeInfo = context.GetTypeInfo(symbol);
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
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

        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var x = context.GetTypeInfo(symbol);
        var y = context.GetTypeInfo(compilation.CreateArrayTypeSymbol(symbol, 1));
        var a = Symbols.GetConstructor(context, x, members);
        var b = Symbols.GetConstructor(context, y, members);
        Assert.Null(a);
        Assert.Null(b);
    }

    public static IEnumerable<object[]> IsRequiredData()
    {
        var a =
            """
            class Alpha
            {
                public int Field;

                public required int RequiredField;

                public string? Property { get; set; }

                public required string? RequiredProperty { get; set; }
            }
            """;
        yield return new object[] { a, "Alpha", "Field", false };
        yield return new object[] { a, "Alpha", "RequiredField", true };
        yield return new object[] { a, "Alpha", "Property", false };
        yield return new object[] { a, "Alpha", "RequiredProperty", true };
    }

    [Theory(DisplayName = "Is Required Field Or Property")]
    [MemberData(nameof(IsRequiredData))]
    public void IsRequiredTest(string source, string typeName, string memberName, bool required)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);
        var member = symbol.GetMembers()
            .Where(x => ((x as IFieldSymbol)?.Name ?? (x as IPropertySymbol)?.Name) == memberName)
            .FirstOrDefault();
        Assert.NotNull(member);
        var actual = Symbols.IsRequiredFieldOrProperty(member);
        Assert.Equal(required, actual);
    }

    public static IEnumerable<object[]> IsReadOnlyData()
    {
        var a =
            """
            class Alpha
            {
                public int Field;

                public readonly int ReadOnlyField;

                public string? Property { get; set; }

                public string? ReadOnlyProperty { get; }
            }
            """;
        yield return new object[] { a, "Alpha", "Field", false };
        yield return new object[] { a, "Alpha", "ReadOnlyField", true };
        yield return new object[] { a, "Alpha", "Property", false };
        yield return new object[] { a, "Alpha", "ReadOnlyProperty", true };
    }

    [Theory(DisplayName = "Is ReadOnly Field Or Property")]
    [MemberData(nameof(IsReadOnlyData))]
    public void IsReadOnlyTest(string source, string typeName, string memberName, bool @readonly)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);
        var member = symbol.GetMembers()
            .Where(x => ((x as IFieldSymbol)?.Name ?? (x as IPropertySymbol)?.Name) == memberName)
            .FirstOrDefault();
        Assert.NotNull(member);
        var actual = Symbols.IsReadOnlyFieldOrProperty(member);
        Assert.Equal(@readonly, actual);
    }

    public static IEnumerable<object[]> FilterFieldsAndPropertiesData()
    {
        var a =
            """
            class Alpha
            {
                public int PublicInstanceField;

                public int PublicInstanceProperty { get; set; }

                private int PrivateInstanceField;

                private int PrivateInstanceProperty { get; set; }

                internal int InternalInstanceField;

                internal int InternalInstanceProperty { get; set; }

                public static int PublicStaticField;

                public static int PublicStaticProperty { get; set; }

                public ref int PublicInstancePropertyReturnsByRef => throw null;

                public ref readonly int PublicInstancePropertyReturnsByRefReadonly => throw null;

                public void PublicInstanceMethod() { }

                public static void PublicStaticMethod() { }

                public int this[int key] => default;

                private long this[long key] => default;

                internal short this[short key] => default;
            }
            """;
        yield return new object[] { a, "Alpha", new string[] { "PublicInstanceField", "PublicInstanceProperty" } };
    }

    [Theory(DisplayName = "Filter Fields And Properties Test")]
    [MemberData(nameof(FilterFieldsAndPropertiesData))]
    public void FilterFieldsAndPropertiesTest(string source, string typeName, string[] expectedMemberNames)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<TypeDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration);
        Assert.NotNull(symbol);
        Assert.Equal(typeName, symbol.Name);
        var members = symbol.GetMembers();
        Assert.Contains(members, x => x is IMethodSymbol);
        Assert.Contains(members, x => x is IFieldSymbol);
        Assert.Contains(members, x => x is IPropertySymbol property && property.IsIndexer);
        Assert.Contains(members, x => x is IPropertySymbol property && property.IsIndexer is false);
        Assert.Contains(members, x => x.IsStatic);
        Assert.Contains(members, x => x.IsStatic is false);
        Assert.Contains(members, x => x.DeclaredAccessibility is Accessibility.Public);
        Assert.Contains(members, x => x.DeclaredAccessibility is not Accessibility.Public);
        Assert.Contains(members, x => x is IPropertySymbol property && property.ReturnsByRef);
        Assert.Contains(members, x => x is IPropertySymbol property && property.ReturnsByRefReadonly);

        var filtered = Symbols.FilterFieldsAndProperties(members, default);
        Assert.Equal(new HashSet<string>(expectedMemberNames), [.. filtered.Select(x => x.Name)]);
    }

    public static IEnumerable<object[]> KeywordData()
    {
        yield return new object[] { "bool", true, "@bool" };
        yield return new object[] { "public", true, "@public" };
        yield return new object[] { "operator", true, "@operator" };

        yield return new object[] { "yield", true, "@yield" };
        yield return new object[] { "var", true, "@var" };
        yield return new object[] { "async", true, "@async" };
        yield return new object[] { "await", true, "@await" };

        yield return new object[] { "list", false, "list" };
        yield return new object[] { "dictionary", false, "dictionary" };
        yield return new object[] { "Class", false, "Class" };
        yield return new object[] { "Namespace", false, "Namespace" };
    }

    [Theory(DisplayName = "Keyword In Source Code Test")]
    [MemberData(nameof(KeywordData))]
    public void KeywordTest(string text, bool keyword, string expected)
    {
        var result = Symbols.IsKeyword(text);
        var actual = Symbols.GetNameInSourceCode(text);
        Assert.Equal(keyword, result);
        Assert.Equal(expected, actual);
    }
}
