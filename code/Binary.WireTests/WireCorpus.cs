namespace Mikodev.Binary.WireTests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Net;
using System.Numerics;
using System.Text;

// `Name` is the cross language key. `Langs` says which implementations can express
// the value, so a row is shared only if all of them assert the same bytes.
[Flags]
public enum Lang
{
    CSharp = 1,
    Cxx = 2,
    Rust = 4,
    All = CSharp | Cxx | Rust,
}

// `Group` only feeds the coverage report.
public sealed record WireCase(string Name, string Group, Type Type, object? Value, Lang Langs = Lang.All);

// Adding a case means one row here plus a regenerated `fixtures/wire.tsv` (`dotnet run --project <workspace>/wire-table-generator`).
public static class WireCorpus
{
    private static readonly Guid SampleGuid = new("47a87bd4-053c-47c5-b970-7e2164b167bc");

    private static readonly IPAddress SampleAddress = IPAddress.Parse("192.168.1.1");

    private static readonly byte[] SampleBytes = [1, 2, 3, 4, 5];

    public static IReadOnlyList<WireCase> All { get; } =
    [
        // A — fixed-width scalars: no prefix, little endian.
        new("bool_true", "A", typeof(bool), true),
        new("bool_false", "A", typeof(bool), false),
        new("u8_0", "A", typeof(byte), (byte)0),
        new("u8_255", "A", typeof(byte), (byte)255),
        new("i8_neg128", "A", typeof(sbyte), (sbyte)-128),
        new("i8_neg1", "A", typeof(sbyte), (sbyte)-1),
        new("i8_127", "A", typeof(sbyte), (sbyte)127),
        new("u16_0", "A", typeof(ushort), (ushort)0),
        new("u16_60000", "A", typeof(ushort), (ushort)60000),
        new("u16_max", "A", typeof(ushort), ushort.MaxValue),
        new("i16_min", "A", typeof(short), short.MinValue),
        new("i16_neg2", "A", typeof(short), (short)-2),
        new("i16_0", "A", typeof(short), (short)0),
        new("i16_max", "A", typeof(short), short.MaxValue),
        new("u32_0", "A", typeof(uint), 0U),
        new("u32_max", "A", typeof(uint), uint.MaxValue),
        new("i32_min", "A", typeof(int), int.MinValue),
        new("i32_neg7", "A", typeof(int), -7),
        new("i32_0", "A", typeof(int), 0),
        new("i32_42", "A", typeof(int), 42),
        new("i32_max", "A", typeof(int), int.MaxValue),
        new("u64_0", "A", typeof(ulong), 0UL),
        new("u64_max", "A", typeof(ulong), ulong.MaxValue),
        new("i64_min", "A", typeof(long), long.MinValue),
        new("i64_neg2", "A", typeof(long), -2L),
        new("i64_0", "A", typeof(long), 0L),
        new("i64_max", "A", typeof(long), long.MaxValue),
        new("u128_0", "A", typeof(UInt128), UInt128.Zero, Lang.All),
        new("u128_max", "A", typeof(UInt128), UInt128.MaxValue, Lang.All),
        new("i128_min", "A", typeof(Int128), Int128.MinValue, Lang.All),
        new("i128_neg1", "A", typeof(Int128), (Int128)(-1), Lang.All),
        new("i128_0", "A", typeof(Int128), Int128.Zero, Lang.All),
        new("i128_max", "A", typeof(Int128), Int128.MaxValue, Lang.All),
        new("f32_0", "A", typeof(float), 0F),
        new("f32_neg_zero", "A", typeof(float), -0.0F),
        new("f32_1_5", "A", typeof(float), 1.5F),
        new("f64_0", "A", typeof(double), 0D),
        new("f64_neg_zero", "A", typeof(double), -0.0D),
        new("f64_1_5", "A", typeof(double), 1.5D),
        new("half_0", "A", typeof(Half), (Half)0F, Lang.CSharp),
        new("half_1_5", "A", typeof(Half), (Half)1.5F, Lang.CSharp),
        new("char_0", "A", typeof(char), '\0', Lang.CSharp),
        new("char_A", "A", typeof(char), 'A', Lang.CSharp),
        new("char_ff", "A", typeof(char), '\u00FF', Lang.CSharp),
        new("index_2", "A", typeof(Index), new Index(2), Lang.CSharp),
        new("bitvector32_1234", "A", typeof(BitVector32), new BitVector32(0x1234), Lang.CSharp),

        // B — fixed-width composites: same rule, several components.
        new("range_1_3", "B", typeof(Range), new Range(1, 3), Lang.CSharp),
        new("complex_1_5", "B", typeof(Complex), new Complex(1.5, -2.25), Lang.CSharp),
        new("vector2", "B", typeof(Vector2), new Vector2(1.5F, -2.5F), Lang.CSharp),
        new("vector3", "B", typeof(Vector3), new Vector3(1.5F, -2.5F, 3.5F), Lang.CSharp),
        new("vector4", "B", typeof(Vector4), new Vector4(1.5F, -2.5F, 3.5F, -4.5F), Lang.CSharp),
        new("matrix3x2", "B", typeof(Matrix3x2), new Matrix3x2(1, 2, 3, 4, 5, 6), Lang.CSharp),
        new("matrix4x4", "B", typeof(Matrix4x4), new Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16), Lang.CSharp),
        new("plane", "B", typeof(Plane), new Plane(1, 2, 3, 4), Lang.CSharp),
        new("quaternion", "B", typeof(Quaternion), new Quaternion(1, 2, 3, 4), Lang.CSharp),

        // C — semantic scalars: fixed width, field order fixed by the type.
        new("dateonly", "C", typeof(DateOnly), new DateOnly(2024, 1, 2), Lang.CSharp),
        new("datetime", "C", typeof(DateTime), new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), Lang.CSharp),
        new("datetimeoffset", "C", typeof(DateTimeOffset), new DateTimeOffset(new DateTime(2024, 1, 2, 3, 4, 5), TimeSpan.FromHours(8)), Lang.CSharp),
        new("timeonly", "C", typeof(TimeOnly), new TimeOnly(3, 4, 5, 6), Lang.CSharp),
        new("timespan", "C", typeof(TimeSpan), TimeSpan.FromMilliseconds(1234.5), Lang.CSharp),
        new("decimal_zero", "C", typeof(decimal), 0M, Lang.CSharp),
        new("decimal_1234", "C", typeof(decimal), 1234.5678M, Lang.CSharp),
        new("decimal_neg", "C", typeof(decimal), -1234.5678M, Lang.CSharp),
        new("decimal_max", "C", typeof(decimal), decimal.MaxValue, Lang.CSharp),
        new("guid", "C", typeof(Guid), SampleGuid, Lang.CSharp),
        new("rune_zh", "C", typeof(Rune), new Rune('中'), Lang.CSharp),

        // D — variable-length byte strings: length prefix + bytes, UTF-8 on wire.
        new("string_empty", "D", typeof(string), string.Empty),
        new("string_16a", "D", typeof(string), new string('a', 16)),
        new("string_17a", "D", typeof(string), new string('a', 17)),
        new("string_41a", "D", typeof(string), new string('a', 41)),
        new("string_42a", "D", typeof(string), new string('a', 42)),
        new("string_127a", "D", typeof(string), new string('a', 127)),
        new("string_128a", "D", typeof(string), new string('a', 128)),
        new("string_20zh", "D", typeof(string), new string('中', 20)),
        new("string_2emoji", "D", typeof(string), "😀😀"),

        // E — variable-length numbers: prefix + library-defined layout.
        new("biginteger_neg", "E", typeof(BigInteger), BigInteger.Parse("-123456789012345678901234567890"), Lang.CSharp),
        new("biginteger_zero", "E", typeof(BigInteger), BigInteger.Zero, Lang.CSharp),
        new("bitarray_1011", "E", typeof(BitArray), new BitArray(new[] { true, false, true, true }), Lang.CSharp),
        new("ipaddress_v4", "E", typeof(IPAddress), SampleAddress, Lang.CSharp),
        new("ipendpoint_v4", "E", typeof(IPEndPoint), new IPEndPoint(SampleAddress, 8080), Lang.CSharp),
        new("version_1234", "E", typeof(Version), new Version(1, 2, 3, 4), Lang.CSharp),
        new("uri", "E", typeof(Uri), new Uri("https://example.com/a?b=c"), Lang.CSharp),

        // F — nullable: tag + auto(value), no prefix of its own.
        new("i32opt_none", "F", typeof(int?), (int?)null),
        new("i32opt_some5", "F", typeof(int?), (int?)5),
        new("named_with_opt_none", "F", typeof(NamedWithOpt), new NamedWithOpt { V = null }),
        new("named_with_opt_some", "F", typeof(NamedWithOpt), new NamedWithOpt { V = 5 }),

        // G — tuples: members flattened, last one raw.
        new("tuple_i32_str", "G", typeof(ValueTuple<int, string>), (1, "a")),
        new("tuple_three", "G", typeof(ValueTuple<int, int, string>), (1, 2, "c")),

        new("tuple_numerics", "G", typeof((sbyte, short, int, long, Int128, UInt128, double, Half)), ((sbyte)-128, short.MinValue, int.MaxValue, long.MinValue, Int128.MinValue, UInt128.MaxValue, -0.0D, (Half)1.5F), Lang.CSharp),

        new("tuple_nested_pair", "G", typeof(((int, string), (int, int))), ((1, "a"), (2, 3))),

        new("tuple_ten_mixed", "G", typeof((sbyte, short, int, long, Int128, UInt128, double, Half, float, decimal)), ((sbyte)-128, short.MinValue, int.MaxValue, long.MinValue, Int128.MinValue, UInt128.MaxValue, -0.0D, (Half)1.5F, 3.5F, 1.5M), Lang.CSharp),

        // H — sequences: prefix + element stream, no element count.
        new("vec_i32_3", "H", typeof(int[]), new[] { 1, 2, 3 }),
        new("vec_i32_5", "H", typeof(int[]), new[] { 1, 2, 3, 4, 5 }),
        new("list_i32_3", "H", typeof(List<int>), new List<int> { 1, 2, 3 }, Lang.CSharp),
        new("linkedlist_i32_3", "H", typeof(LinkedList<int>), new LinkedList<int>([1, 2, 3]), Lang.CSharp),
        new("linkedlist_i32_5", "H", typeof(LinkedList<int>), new LinkedList<int>([1, 2, 3, 4, 5]), Lang.CSharp),
        new("queue_i32_3", "H", typeof(Queue<int>), new Queue<int>([1, 2, 3]), Lang.CSharp),
        new("hashset_i32_3", "H", typeof(HashSet<int>), new HashSet<int> { 1, 2, 3 }, Lang.CSharp),
        new("immutablelist_i32_3", "H", typeof(ImmutableList<int>), ImmutableList.Create(1, 2, 3), Lang.CSharp),
        new("arraysegment_i32_3", "H", typeof(ArraySegment<int>), new ArraySegment<int>([1, 2, 3]), Lang.CSharp),
        new("memory_i32_3", "H", typeof(Memory<int>), new Memory<int>([1, 2, 3]), Lang.CSharp),
        new("readonlymemory_i32_3", "H", typeof(ReadOnlyMemory<int>), new ReadOnlyMemory<int>([1, 2, 3]), Lang.CSharp),
        new("vec_string_4x4", "H", typeof(string[]), new[] { "aaaa", "bbbb", "cccc", "dddd" }),
        new("list_string_4x4", "H", typeof(List<string>), new List<string> { "aaaa", "bbbb", "cccc", "dddd" }, Lang.CSharp),
        new("array2d_i32", "H", typeof(int[,]), new int[,] { { 1, 2 }, { 3, 4 } }, Lang.CSharp),
        new("vec_vec_i32", "H", typeof(List<int[]>), new List<int[]> { new[] { 1 }, new[] { 2 } }),
        new("vec_i32opt", "H", typeof(int?[]), new int?[] { null, 5 }),
        new("vec_i32opt_none", "H", typeof(int?[]), new int?[] { null }),
        new("sortedset_i32_5", "H", typeof(SortedSet<int>), new SortedSet<int> { 1, 2, 3, 4, 5 }),

        // I — mappings: each entry is a flattened (K, V).
        new("dictionary_i32_str", "I", typeof(Dictionary<int, string>), new Dictionary<int, string> { [1] = "a", [2] = "b" }, Lang.CSharp),
        new("sorteddictionary_i32_str", "I", typeof(SortedDictionary<int, string>), new SortedDictionary<int, string> { [1] = "a", [2] = "b" }),
        new("sorteddictionary_i32_i32_5", "I", typeof(SortedDictionary<int, int>), new SortedDictionary<int, int> { [1] = 1, [2] = 2, [3] = 3, [4] = 4, [5] = 5 }),
        new("sorteddictionary_i32_str_4", "I", typeof(SortedDictionary<int, string>), new SortedDictionary<int, string> { [1] = "a", [2] = "b", [3] = "c", [4] = "d" }),

        // J — key/value pair: two members flattened.
        new("kvp_i32_str", "J", typeof(KeyValuePair<int, string>), new KeyValuePair<int, string>(1, "a"), Lang.All),

        // K — enum: the underlying integer, nothing else.
        new("enum_green", "K", typeof(Color), Color.Green),
        new("vec_enum", "K", typeof(Color[]), new[] { Color.Red, Color.Green }),

        // M — objects: names + length-prefixed members, or tuple-object flattening.
        new("named_tiny", "M", typeof(NamedTiny), new NamedTiny { X = 7 }),
        new("named_person", "M", typeof(NamedPerson), new NamedPerson { Id = 42, Name = "Bob" }),
        new("named_mid", "M", typeof(NamedMid), new NamedMid { A = 1, B = 2, C = 3 }),
        new("tupleobj_two_int", "M", typeof(TupleTwoInt), new TupleTwoInt { A = 1, B = 2 }),
        new("tupleobj_int_str", "M", typeof(TupleIntStr), new TupleIntStr { A = 1, B = "a" }),

        // Raw byte containers: element width is 1, so the prefix is the whole story.
        new("bytes_5", "H", typeof(byte[]), SampleBytes, Lang.All),
    ];
}
