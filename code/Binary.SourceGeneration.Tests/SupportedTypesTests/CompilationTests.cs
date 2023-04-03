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

            [SourceGeneratorContext]
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

    public static IEnumerable<object[]> SystemTupleData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<(int, int)>]
            [SourceGeneratorInclude<(int, string, Guid)>]
            [SourceGeneratorInclude<(byte, sbyte, ushort, short, uint, int, ulong, long, UInt128, Int128)>]
            [SourceGeneratorInclude<Tuple<short, long>>]
            [SourceGeneratorInclude<Tuple<short, long, string>>]
            [SourceGeneratorInclude<Tuple<byte, sbyte, ushort, short, uint, int, ulong, Tuple<long, UInt128, Int128>>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CustomConverterData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            public class FakeConverter<T> : Converter<T>
            {
                public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

                public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();
            }

            [Converter(typeof(FakeConverter<Alpha>))]
            public class Alpha { }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            public class FakeConverter<T> : Converter<T>
            {
                public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

                public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();
            }

            public class Bravo
            {
                [Converter(typeof(FakeConverter<int>))]
                public int Data { get; set; }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
    }

    public static IEnumerable<object[]> CustomConverterCreatorData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            public class FakeConverterCreator<T> : IConverterCreator
            {
                public IConverter? GetConverter(IGeneratorContext context, Type type) => throw new NotSupportedException();
            }

            [ConverterCreator(typeof(FakeConverterCreator<Alpha>))]
            public class Alpha { }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            public class FakeConverterCreator<T> : IConverterCreator
            {
                public IConverter? GetConverter(IGeneratorContext context, Type type) => throw new NotSupportedException();
            }

            public class Bravo
            {
                [ConverterCreator(typeof(FakeConverterCreator<string>))]
                public string Data { get; set; }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
    }

    public static IEnumerable<object[]> CustomNamedObjectData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("field")]
                public int Field;

                [NamedKey("ro-property")]
                public string ROProperty { get; }

                [NamedKey("rw-property")]
                public string RWProperty { get; set; }

                public Alpha(int field, string rOProperty, string rWProperty)
                {
                    this.Field = field;
                    ROProperty = rOProperty;
                    RWProperty = rWProperty;
                }
            }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Bravo
            {
                [NamedKey("id")]
                public int Id { get; internal set; }
            }
            """;
        var c =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Delta
            {
                [NamedKey("opt-field")]
                public int OField;

                [NamedKey("req-field")]
                public required int RField;

                [NamedKey("opt-property")]
                public string OProperty { get; set; }

                [NamedKey("req-property")]
                public required string RProperty { get; set; }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
    }

    public static IEnumerable<object[]> CustomTupleObjectData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public class Alpha
            {
                [TupleKey(0)]
                public int Field;

                [TupleKey(1)]
                public string ROProperty { get; }

                [TupleKey(2)]
                public string RWProperty { get; set; }

                public Alpha(int field, string rOProperty, string rWProperty)
                {
                    this.Field = field;
                    ROProperty = rOProperty;
                    RWProperty = rWProperty;
                }
            }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                public int Id { get; }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
    }

    public static IEnumerable<object[]> CustomPlainObjectData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Plain>]
            public partial class TestGeneratorContext { }

            public class Plain
            {
                public int Id { get; }

                public string Name { get; set; }

                public Plain(int id)
                {
                    Id = id;
                }
            }
            """;
        yield return new object[] { a };
    }

    [Theory(DisplayName = "Compilation Test")]
    [MemberData(nameof(SpanLikeTypesData))]
    [MemberData(nameof(CommonGenericTypesData))]
    [MemberData(nameof(CommonCollectionTypesData))]
    [MemberData(nameof(CommonCollectionInterfaceTypesData))]
    [MemberData(nameof(ImmutableCollectionTypesData))]
    [MemberData(nameof(SystemTupleData))]
    [MemberData(nameof(CustomConverterData))]
    [MemberData(nameof(CustomConverterCreatorData))]
    [MemberData(nameof(CustomNamedObjectData))]
    [MemberData(nameof(CustomTupleObjectData))]
    [MemberData(nameof(CustomPlainObjectData))]
    public void CompilationTest(string source)
    {
        Assert.Contains("SourceGeneratorContext", source);
        Assert.Contains("SourceGeneratorInclude", source);
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
