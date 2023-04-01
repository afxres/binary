namespace Mikodev.Binary.SourceGeneration.Tests.SupportedTypesTests;

using System.Collections.Generic;
using Xunit;

public class CompilationTests
{
    public static IEnumerable<object[]> SpanLikeTypesData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<List<int>>]
            [SourceGeneratorInclude<Memory<string>>]
            [SourceGeneratorInclude<ArraySegment<int>>]
            [SourceGeneratorInclude<ReadOnlyMemory<string>>]
            [SourceGeneratorInclude<ImmutableArray<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CommonGenericTypesData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Buffers;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<KeyValuePair<string, int>>]
            [SourceGeneratorInclude<LinkedList<string>>]
            [SourceGeneratorInclude<Nullable<int>>]
            [SourceGeneratorInclude<PriorityQueue<string, int>>]
            [SourceGeneratorInclude<ReadOnlySequence<string>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CommonCollectionTypesData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Dictionary<string, int>>]
            [SourceGeneratorInclude<List<string>>]
            [SourceGeneratorInclude<HashSet<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CommonCollectionInterfaceTypesData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<IList<string>>]
            [SourceGeneratorInclude<ICollection<int>>]
            [SourceGeneratorInclude<IEnumerable<string>>]
            [SourceGeneratorInclude<IReadOnlyList<int>>]
            [SourceGeneratorInclude<IReadOnlyCollection<string>>]
            [SourceGeneratorInclude<ISet<int>>]
            [SourceGeneratorInclude<IReadOnlySet<string>>]
            [SourceGeneratorInclude<IDictionary<int, string>>]
            [SourceGeneratorInclude<IReadOnlyDictionary<long, string>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> ImmutableCollectionTypesData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System.Collections.Immutable;

            [SourceGeneratorInclude<IImmutableDictionary<int, string>>]
            [SourceGeneratorInclude<IImmutableList<int>>]
            [SourceGeneratorInclude<IImmutableQueue<string>>]
            [SourceGeneratorInclude<IImmutableSet<int>>]
            [SourceGeneratorInclude<ImmutableDictionary<string, int>>]
            [SourceGeneratorInclude<ImmutableHashSet<string>>]
            [SourceGeneratorInclude<ImmutableList<int>>]
            [SourceGeneratorInclude<ImmutableQueue<string>>]
            [SourceGeneratorInclude<ImmutableSortedDictionary<int, string>>]
            [SourceGeneratorInclude<ImmutableSortedSet<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    [Theory(DisplayName = "Compilation Test")]
    [MemberData(nameof(SpanLikeTypesData))]
    [MemberData(nameof(CommonGenericTypesData))]
    [MemberData(nameof(CommonCollectionTypesData))]
    [MemberData(nameof(CommonCollectionInterfaceTypesData))]
    [MemberData(nameof(ImmutableCollectionTypesData))]
    public void CompilationTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
