namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequiredSupportedTypeTests
{
    public static IEnumerable<object[]> RequireSupportedTypeForIncludeAttributeData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Action>]
            public partial class TestGeneratorContext { }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Predicate<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a, "Action" };
        yield return new object[] { b, "Predicate<Int32>" };
    }

    [Theory(DisplayName = "Require Supported Type For Include Attribute")]
    [MemberData(nameof(RequireSupportedTypeForIncludeAttributeData))]
    public void RequireSupportedTypeForIncludeAttributeTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require supported type (array, class, enum, interface or struct), type: {typeName}", diagnostic.ToString());
        Assert.Matches(@"SourceGeneratorInclude<.*>", diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> RequireSupportedTypeForMemberData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("a")]
                public int Id { get; set; }

                [NamedKey("b")]
                public Comparison<string> Method { get; set; }
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public unsafe class Bravo
            {
                [TupleKey(0)]
                public int* Function;

                [TupleKey(1)]
                public string Name;
            }
            """;
        var c =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta<string>>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public unsafe class Delta<T>
            {
                [NamedKey("data")]
                public T Data;

                [NamedKey("pointer")]
                public delegate*<T> Pointer { get; set; }
            }
            """;
        var d =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Hotel>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Hotel
            {
                [NamedKey("id")]
                public string Id;

                [NamedKey("span")]
                public Span<byte> Buffer => throw new NotSupportedException();
            }
            """;
        yield return new object[] { a, "Comparison<String>", "Method", "Alpha" };
        yield return new object[] { b, "Int32*", "Function", "Bravo" };
        yield return new object[] { c, "delegate*<String>", "Pointer", "Delta<String>" };
        yield return new object[] { d, "Span<Byte>", "Buffer", "Hotel" };
    }

    [Theory(DisplayName = "Require Supported Type For Member")]
    [MemberData(nameof(RequireSupportedTypeForMemberData))]
    public void RequireSupportedTypeForMemberTest(string source, string typeName, string memberName, string containingTypeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require supported type (array, class, enum, interface or struct), type: {typeName}, member name: {memberName}, containing type: {containingTypeName}", diagnostic.ToString());
        Assert.Contains(memberName, diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> RequireSupportedTypeForMemberPlainObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Plain>]
            public partial class TestGeneratorContext { }

            public unsafe class Plain
            {
                public int Id;

                public int* Location;
            }
            """;
        yield return new object[] { a };
    }

    [Theory(DisplayName = "Require Supported Type For Member Of Plain Object")]
    [MemberData(nameof(RequireSupportedTypeForMemberPlainObjectData))]
    public void RequireSupportedTypeForMemberPlainObjectTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
