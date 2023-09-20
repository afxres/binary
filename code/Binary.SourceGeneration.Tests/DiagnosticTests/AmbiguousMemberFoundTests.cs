namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

public class AmbiguousMemberFoundTests
{
    public static IEnumerable<object[]> AmbiguousInterfaceData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<INameChild>]
            public partial class TestGeneratorContext { }

            interface INameA
            {
                string Name { get; }
            }

            interface INameB
            {
                string Name { get; }
            }

            interface INameChild : INameA, INameB { }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<IMultipleChild>]
            public partial class TestGeneratorContext { }

            interface IMultipleA
            {
                int A { get; }

                double B { get; }

                string C { get; }
            }

            interface IMultipleB
            {
                int B { get; }

                string C { get; }

                Guid D { get; }
            }
            interface IMultipleChild : IMultipleA, IMultipleB { }
            """;
        yield return new object[] { a, new string[] { "Name" }, "INameChild" };
        yield return new object[] { b, new string[] { "B", "C" }, "IMultipleChild" };
    }

    [Theory(DisplayName = "Ambiguous Member Found")]
    [MemberData(nameof(AmbiguousInterfaceData))]
    public void AmbiguousMemberTest(string source, IReadOnlyCollection<string> memberNames, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Equal(memberNames.Count, diagnostics.Length);
        var expected = new Regex("Ambiguous member found, member name: (\\w*), type: (\\S*)$");
        var actualNames = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            var matches = expected.Matches(diagnostic.ToString());
            var match = Assert.Single(matches);
            Assert.Equal(3, match.Groups.Count);
            Assert.Contains(match.Groups[1].Value, memberNames);
            Assert.Equal(typeName, match.Groups[2].Value);
            Assert.Equal($"SourceGeneratorInclude<{typeName}>", diagnostic.Location.GetSourceText());
            actualNames.Add(match.Groups[1].Value);
        }
        Assert.Equal(memberNames.ToHashSet(), actualNames.ToHashSet());
    }
}
