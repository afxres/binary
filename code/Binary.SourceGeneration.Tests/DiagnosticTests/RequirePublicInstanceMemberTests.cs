namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequirePublicInstanceMemberTests
{
    public static IEnumerable<object[]> NonPublicMemberData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("private")]
                private int PrivateProperty { get; set; }
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class BravoSourceGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                internal string? InternalField;
            }
            """;
        yield return new object[] { a, "PrivateProperty", "Alpha" };
        yield return new object[] { b, "InternalField", "Bravo" };
    }

    public static IEnumerable<object[]> NonInstanceMemberData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("static")]
                public static int StaticProperty { get; set; }
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class BravoSourceGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                public static string? StaticField;
            }
            """;
        yield return new object[] { a, "StaticProperty", "Alpha" };
        yield return new object[] { b, "StaticField", "Bravo" };
    }

    public static IEnumerable<object[]> NonPublicInstanceMemberData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("invalid")]
                private static int PrivateStaticProperty { get; set; }
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class BravoSourceGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                internal static string? InternalStaticField;
            }
            """;
        yield return new object[] { a, "PrivateStaticProperty", "Alpha" };
        yield return new object[] { b, "InternalStaticField", "Bravo" };
    }

    [Theory(DisplayName = "Require Public Instance Member")]
    [MemberData(nameof(NonPublicMemberData))]
    [MemberData(nameof(NonInstanceMemberData))]
    [MemberData(nameof(NonPublicInstanceMemberData))]
    public void RequirePublicInstanceTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require public instance member, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Contains(memberName, diagnostic.Location.GetSourceText());
    }
}
