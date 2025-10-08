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
        yield return [a, "PrivateProperty", "Alpha"];
        yield return [b, "InternalField", "Bravo"];
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
        var c =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            partial class TestSourceGeneratorContext { }

            [NamedObject]
            class Delta
            {
                [NamedKey("constant")]
                public const int Constant = 1;
            }
            """;
        yield return [a, "StaticProperty", "Alpha"];
        yield return [b, "StaticField", "Bravo"];
        yield return [c, "Constant", "Delta"];
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
        yield return [a, "PrivateStaticProperty", "Alpha"];
        yield return [b, "InternalStaticField", "Bravo"];
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
