namespace Mikodev.Binary.SourceGeneration.Tests.SupportedTypesTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CompilationTests
{
    public static IEnumerable<object[]> SpanLikeTypesData()
    {
        var a =
            """
            // span-like collections
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<int[]>]
            [SourceGeneratorInclude<string[]>]
            [SourceGeneratorInclude<List<int>>]
            [SourceGeneratorInclude<Memory<string>>]
            [SourceGeneratorInclude<ArraySegment<int>>]
            [SourceGeneratorInclude<ReadOnlyMemory<string>>]
            [SourceGeneratorInclude<ImmutableArray<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> EnumTypesData()
    {
        var a =
            """
            // enum types
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<DayOfWeek>]
            [SourceGeneratorInclude<DateTimeKind>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CommonGenericTypesData()
    {
        var a =
            """
            // common generic types
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
            // common collections
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
            // common collection interfaces
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

    public static IEnumerable<object[]> FrozenCollectionTypesData()
    {
        var a =
            """
            // frozen collections
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System.Collections.Frozen;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<FrozenDictionary<int, string>>]
            [SourceGeneratorInclude<FrozenSet<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> ImmutableCollectionTypesData()
    {
        var a =
            """
            // immutable collections
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
            // system tuple types
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
            // custom converter on type
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
            // custom converter on member
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

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
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
            // custom converter creator on type
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
            // custom converter creator on member
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

            [NamedObject]
            public class Bravo
            {
                [NamedKey("data")]
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
            // named object
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
            // named object with non-public setter
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
            // named object with required members
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
        var d =
            """
            // custom named object with ignored members
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Hotel>]
            partial class TestGeneratorContext { }

            [NamedObject]
            class Hotel
            {
                [NamedKey("id")]
                public int Id;

                public double Ignore;

                [NamedKey("tag")]
                public string Tag { get; set; }

                public short Hidden { get; set; }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
        yield return new object[] { d };
    }

    public static IEnumerable<object[]> CustomTupleObjectData()
    {
        var a =
            """
            // tuple object
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
            // tuple object with readonly property
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
        var c =
            """
            // custom tuple object with ignored members
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            partial class TestGeneratorContext { }

            [TupleObject]
            class Delta
            {
                [TupleKey(0)]
                public int Integer;

                public byte Ignore { get; set; }

                [TupleKey(1)]
                public long Result { get; set; }

                public short Padding;

                [TupleKey(2)]
                public short Tail;
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
    }

    public static IEnumerable<object[]> CustomPlainObjectData()
    {
        var a =
            """
            // plain object
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
        var b =
            """
            // plain object with non-public setter
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<NoSuitableConstructor>]
            public partial class TestGeneratorContext { }

            public class NoSuitableConstructor
            {
                public int Id { get; }

                public string Name { get; }

                public NoSuitableConstructor(int id)
                {
                    Id = id;
                }
            }
            """;
        var c =
            """
            // plain object with miscellaneous members
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<MiscellaneousMembers>]
            public partial class TestGeneratorContext { }

            public class MiscellaneousMembers
            {
                internal const int InternalConstant = 0;

                public const long Constant = -1;

                public int Id;

                public string Name { get; set; }

                internal int Internal;

                private string Private { get; set; }

                public static int PublicStatic;

                internal static string InternalStatic { get; set; }

                public int this[int key] => key;

                internal string this[string key] => key;
            }
            """;
        var d =
            """
            // plain object member name conflicts
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<SameName>]
            public partial class TestGeneratorContext { }

            public class SameName
            {
                public int ID { get; }

                public string Id { get; }
            }
            """;
        var e =
            """
            // plain object without suitable constructor
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<ConstructorTypeMismatch>]
            public partial class TestGeneratorContext { }

            public class ConstructorTypeMismatch
            {
                public int Id { get; set; }

                public string Number { get; set; }

                public ConstructorTypeMismatch(string id, int number) => throw new System.NotSupportedException();
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
        yield return new object[] { d };
        yield return new object[] { e };
    }

    public static IEnumerable<object[]> ContextWithMiscellaneousAttributesData()
    {
        var a =
            """
            // context with miscellaneous attributes
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections.Generic;

            [AttributeUsage(AttributeTargets.All)]
            public sealed class TestAttribute<T> : Attribute { }

            [TestAttribute<int>]
            [SourceGeneratorContext]
            [SourceGeneratorInclude<List<int>>]
            public partial class TestGeneratorContext { }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> NamedTupleData()
    {
        var a =
            """
            // named tuple
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<(int, string)>]
            [SourceGeneratorInclude<TypeWithNamedTupleAlpha>]
            public partial class TestGeneratorContext { }

            public class TypeWithNamedTupleAlpha
            {
                public (int Id, string Name) Person;
            }
            """;
        yield return new object[] { a };
    }

    public static IEnumerable<object[]> CustomCollectionData()
    {
        var a =
            """
            // custom enumerable
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<CustomEnumerable<int>>]
            partial class TestGeneratorContext { }

            class CustomEnumerable<T> : IEnumerable<T>
            {
                public CustomEnumerable(IEnumerable<T> values) => throw new NotSupportedException();

                IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

                IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException();
            }
            """;
        var b =
            """
            // custom dictionary
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<CustomDictionary<int, string>>]
            partial class TestGeneratorContext { }

            class CustomDictionary<K, V> : IDictionary<K, V>
            {
                public CustomDictionary(IDictionary<K, V> values) => throw new NotSupportedException();

                void IDictionary<K, V>.Add(K key, V value) => throw new NotSupportedException();

                bool IDictionary<K, V>.ContainsKey(K key) => throw new NotSupportedException();

                bool IDictionary<K, V>.Remove(K key) => throw new NotSupportedException();

                bool IDictionary<K, V>.TryGetValue(K key, out V value) => throw new NotSupportedException();

                V IDictionary<K, V>.this[K key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

                ICollection<K> IDictionary<K, V>.Keys => throw new NotSupportedException();

                ICollection<V> IDictionary<K, V>.Values => throw new NotSupportedException();

                void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => throw new NotSupportedException();

                void ICollection<KeyValuePair<K, V>>.Clear() => throw new NotSupportedException();

                bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item) => throw new NotSupportedException();

                void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => throw new NotSupportedException();

                bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item) => throw new NotSupportedException();

                int ICollection<KeyValuePair<K, V>>.Count => throw new NotSupportedException();

                bool ICollection<KeyValuePair<K, V>>.IsReadOnly => throw new NotSupportedException();

                IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => throw new NotSupportedException();

                IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
            }
            """;
        var c =
            """
            // custom readonly dictionary
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;
            using System.Collections;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<CustomReadOnlyDictionary<string, int>>]
            partial class TestGeneratorContext { }

            class CustomReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V>
            {
                public CustomReadOnlyDictionary(IReadOnlyDictionary<K, V> values) => throw new NotSupportedException();

                bool IReadOnlyDictionary<K, V>.ContainsKey(K key) => throw new NotSupportedException();

                bool IReadOnlyDictionary<K, V>.TryGetValue(K key, out V value) => throw new NotSupportedException();

                V IReadOnlyDictionary<K, V>.this[K key] => throw new NotSupportedException();

                IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => throw new NotSupportedException();

                IEnumerable<V> IReadOnlyDictionary<K, V>.Values => throw new NotSupportedException();

                int IReadOnlyCollection<KeyValuePair<K, V>>.Count => throw new NotSupportedException();

                IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => throw new NotSupportedException();

                IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
    }

    public static IEnumerable<object[]> CustomInterfaceOrAbstractCollectionData()
    {
        var a =
            """
            // custom enumerable interface
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<ICustomEnumerable<int>>]
            partial class TestGeneratorContext { }

            interface ICustomEnumerable<T> : IEnumerable<T> { }
            """;
        var b =
            """
            // custom dictionary interface
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<ICustomDictionary<int, string>>]
            partial class TestGeneratorContext { }

            interface ICustomDictionary<K, V> : IDictionary<K, V> { }
            """;
        var c =
            """
            // custom readonly dictionary interface
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Collections.Generic;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<ICustomReadOnlyDictionary<string, int>>]
            partial class TestGeneratorContext { }

            interface ICustomReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V> { }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
    }

    [Theory(DisplayName = "Compilation Test")]
    [MemberData(nameof(SpanLikeTypesData))]
    [MemberData(nameof(EnumTypesData))]
    [MemberData(nameof(CommonGenericTypesData))]
    [MemberData(nameof(CommonCollectionTypesData))]
    [MemberData(nameof(CommonCollectionInterfaceTypesData))]
    [MemberData(nameof(FrozenCollectionTypesData))]
    [MemberData(nameof(ImmutableCollectionTypesData))]
    [MemberData(nameof(SystemTupleData))]
    [MemberData(nameof(CustomConverterData))]
    [MemberData(nameof(CustomConverterCreatorData))]
    [MemberData(nameof(CustomNamedObjectData))]
    [MemberData(nameof(CustomTupleObjectData))]
    [MemberData(nameof(CustomPlainObjectData))]
    [MemberData(nameof(ContextWithMiscellaneousAttributesData))]
    [MemberData(nameof(NamedTupleData))]
    [MemberData(nameof(CustomCollectionData))]
    [MemberData(nameof(CustomInterfaceOrAbstractCollectionData))]
    public void CompilationTest(string source)
    {
        Assert.Contains("SourceGeneratorContext", source);
        Assert.Contains("SourceGeneratorInclude", source);
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        var compilationGenerated = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var syntaxTrees = compilationGenerated.SyntaxTrees;
        var filePaths = syntaxTrees.Select(x => x.FilePath).ToList();
        _ = Assert.Single(filePaths, x => x.EndsWith("GeneratorContext.0.g.cs"));
        Assert.Empty(diagnostics);
    }
}
