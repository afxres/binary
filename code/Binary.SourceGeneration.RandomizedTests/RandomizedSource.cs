namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using System;
using System.Collections.Generic;

internal enum RandomizedObjectKind
{
    /// <summary>A type without any attribute, every public member is a named member.</summary>
    Plain,

    /// <summary>A type decorated with <c>NamedObject</c>, keys come from <c>NamedKey</c>.</summary>
    Named,

    /// <summary>A type decorated with <c>TupleObject</c>, keys come from <c>TupleKey</c>.</summary>
    Tuple,
}

internal sealed class RandomizedMember
{
    public required string Name { get; init; }

    /// <summary>The named key, empty for tuple objects which use <see cref="TupleIndex"/> instead.</summary>
    public required string Key { get; init; }

    /// <summary>The type name as written in source, either a primitive or a generated type name.</summary>
    public required string TypeName { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsField { get; init; }

    /// <summary>The position declared by <c>TupleKey</c>, unused for other kinds.</summary>
    public required int TupleIndex { get; init; }

    /// <summary>The embedded tuple object, null when this member is a primitive.</summary>
    public RandomizedType? Nested { get; init; }

    public bool IsNested => this.Nested is not null;
}

internal sealed class RandomizedType
{
    public required string Name { get; init; }

    public required RandomizedObjectKind Kind { get; init; }

    public required bool IsStruct { get; init; }

    public required IReadOnlyList<RandomizedMember> Members { get; init; }
}

internal static class RandomKeys
{
    /// <summary>
    /// Every bucket the generated lookup table distinguishes, plus the lengths where the wire
    /// format switches from a one byte length prefix to the four byte form at 128 bytes, and the
    /// sizes around the vector width of <c>SpanHelpers.SequenceEqual</c>.
    /// </summary>
    private static readonly int[] Lengths =
    [
        1, 2, 3, 4, 5, 6, 7, 8, 9, 12, 15, 16, 17, 20, 24,
        31, 32, 33, 48, 63, 64, 65, 96, 127, 128, 129, 200, 257, 400,
    ];

    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    private const string Alphanumeric = "abcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>Long lengths only, so a cluster always lands on the sequence comparison path.</summary>
    private static readonly int[] LongClusterLengths = [200, 257, 400];

    /// <summary>Lengths around the point where the sequence comparison stops being inlined.</summary>
    private static readonly int[] ShortClusterLengths = [16, 20, 24, 31, 32];

    /// <summary>Multiples of three, so the three byte characters of the key fill it exactly.</summary>
    private static readonly int[] MultibyteClusterLengths = [120, 135, 201];

    /// <summary>
    /// Picks a key length from every bucket the generated lookup table distinguishes, and half of the
    /// time reuses the previous key's first byte or first two bytes so that a bucket holds several
    /// candidates sharing a prefix.
    /// </summary>
    public static string Next(Random random, ref string? previous, HashSet<string> used, bool startsUpperCase = false)
    {
        for (; ; )
        {
            var length = Lengths[random.Next(Lengths.Length)];
            var buffer = new char[length];
            // a plain type uses the member name as its key, so the name starts with an upper case
            // letter: it stays an identifier and never collides with a keyword
            buffer[0] = startsUpperCase ? char.ToUpperInvariant(Letters[random.Next(Letters.Length)]) : Letters[random.Next(Letters.Length)];
            for (var i = 1; i < length; i++)
                buffer[i] = Alphanumeric[random.Next(Alphanumeric.Length)];
            if (previous is not null && random.Next(2) is 0)
                buffer[0] = previous[0];
            if (previous is not null && previous.Length > 1 && random.Next(3) is 0)
            {
                buffer[0] = previous[0];
                if (length > 1)
                    buffer[1] = previous[1];
            }
            var key = new string(buffer);
            if (used.Add(key))
            {
                previous = key;
                return key;
            }
        }
    }

    /// <summary>Keys far past the sequence comparison threshold, four byte length prefix on the wire.</summary>
    public static List<string> LongCluster(Random random, int count, HashSet<string> used, bool startsUpperCase = false)
        => Cluster(random, count, LongClusterLengths[random.Next(LongClusterLengths.Length)], used, multibyte: false, startsUpperCase);

    /// <summary>Keys just above the word comparison threshold, around the vector inline limit.</summary>
    public static List<string> ShortCluster(Random random, int count, HashSet<string> used, bool startsUpperCase = false)
        => Cluster(random, count, ShortClusterLengths[random.Next(ShortClusterLengths.Length)], used, multibyte: false, startsUpperCase);

    /// <summary>
    /// Keys made of three byte characters, so the byte length the lookup table buckets by no longer
    /// matches the character count of the key.
    /// </summary>
    public static List<string> MultibyteCluster(Random random, int count, HashSet<string> used)
        => Cluster(random, count, MultibyteClusterLengths[random.Next(MultibyteClusterLengths.Length)], used, multibyte: true);

    /// <summary>
    /// Builds a group of keys sharing the byte length, the first byte and every byte except the last
    /// one. The generated table keeps a single comparison chain per length and first byte, so looking
    /// up a later candidate runs one full comparison per earlier candidate, and a lone key can never
    /// reach that path.
    /// </summary>
    private static List<string> Cluster(Random random, int count, int length, HashSet<string> used, bool multibyte, bool startsUpperCase = false)
    {
        // a three byte character keeps every key of the group in the same length bucket after the
        // final character is swapped
        var size = multibyte ? length / 3 : length;
        var buffer = new char[size];
        if (multibyte)
        {
            for (var i = 0; i < size; i++)
                buffer[i] = (char)(0x4E00 + random.Next(0x1000));
        }
        else
        {
            buffer[0] = startsUpperCase ? char.ToUpperInvariant(Letters[random.Next(Letters.Length)]) : Letters[random.Next(Letters.Length)];
            for (var i = 1; i < size; i++)
                buffer[i] = Alphanumeric[random.Next(Alphanumeric.Length)];
        }
        var results = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            // only the final character differs, everything compared first is identical, and the caller
            // keeps the count well below the alphabet size
            buffer[size - 1] = multibyte ? (char)(0x4E00 + i) : Alphanumeric[i + 1];
            var key = new string(buffer);
            if (used.Add(key))
                results.Add(key);
        }
        return results;
    }
}
