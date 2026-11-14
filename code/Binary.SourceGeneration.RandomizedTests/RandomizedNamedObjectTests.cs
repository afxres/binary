namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using Mikodev.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

/// <summary>
/// Compiles randomly shaped plain and named object types at run time and checks the generated
/// converter against the reflection one. Plain types carry no attribute and use their member names
/// as keys, named types use <c>NamedKey</c>; both kinds share the same set of assertions.
/// </summary>
public class RandomizedNamedObjectTests
{
    private const int TypeCount = 6;

    [Theory(DisplayName = "Randomized Plain And Named Object Test")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void RandomizedNamedObjectTest(int seed)
    {
        var source = RandomizedNamedObjectSource.Create(seed, TypeCount, out var types);
        Assert.Contains(types, x => x.Kind is RandomizedObjectKind.Plain);
        Assert.Contains(types, x => x.Kind is RandomizedObjectKind.Named);
        // every type carries a key past 127 bytes, so the four byte length prefix and the long
        // sequence comparison are both exercised by each seed
        Assert.All(types, x => Assert.Contains(x.Members, y => Encoding.UTF8.GetByteCount(y.Key) > 127));
        // and three clusters share their length and first byte, so the table has to walk a comparison
        // chain inside one bucket instead of matching the only candidate
        Assert.All(types, AssertKeyCluster);
        using var assembly = DynamicAssembly.Create(source, $"RandomizedNamedObject{seed}");
        var creators = assembly.GetConverterCreators("Randomized.RandomizedGeneratorContext");
        var generated = Generator.CreateAotBuilder().AddConverterCreators(creators.Values).Build();
        var reflected = Generator.CreateDefault();
        foreach (var plan in types)
            Validate(assembly.GetType(plan.Name), plan, generated, reflected);
    }

    /// <summary>
    /// Checks that one bucket holds several candidates: three keys sharing the byte length and the
    /// first byte. The generated table keeps one comparison chain per bucket, so only such a bucket
    /// walks past the first candidate. Three groups are required: a long one that also uses the four
    /// byte length prefix, a short one around the vector inline limit, and one made of three byte
    /// characters where the byte length is not the character count.
    /// </summary>
    private static void AssertKeyCluster(RandomizedType plan)
    {
        var buckets = plan.Members.GroupBy(x => (ByteLength: Encoding.UTF8.GetByteCount(x.Key), FirstByte: Encoding.UTF8.GetBytes(x.Key)[0]));
        Assert.Contains(buckets, x => x.Key.ByteLength > 127 && x.Count() >= 3);
        Assert.Contains(buckets, x => x.Key.ByteLength is >= 16 and <= 32 && x.Count() >= 3);
        Assert.Contains(buckets, x => x.Key.ByteLength > 15 && x.Count() >= 3 && x.Any(y => y.Key.Length != x.Key.ByteLength));
    }

    private static void Validate(Type type, RandomizedType plan, IGenerator generated, IGenerator reflected)
    {
        var converter = generated.GetConverter(type);
        var reference = reflected.GetConverter(type);
        var members = plan.Members;
        var item = Activator.CreateInstance(type)!;
        for (var i = 0; i < members.Count; i++)
            SetValue(item, members[i], CreateValue(members[i], i));

        var wire = converter.Encode(item);
        Assert.Equal(converter.Length, reference.Length);
        Assert.Equal(reference.Encode(item), wire);

        var decoded = converter.Decode(wire)!;
        foreach (var member in members)
            Assert.Equal(GetValue(item, member), GetValue(decoded, member));
        Assert.Equal(wire, converter.Encode(decoded));

        ValidateChildren(generated, plan, item, wire);
        ValidateOptionalMembers(type, plan, converter, generated);
        ValidateReordered(converter, generated, item, plan, wire);
        ValidateMissingKey(converter, generated, plan, wire);
        ValidateUnknownKey(converter, generated, plan, wire, decoded);
        ValidateNearMissKeys(converter, generated, plan, wire, decoded);
    }

    /// <summary>
    /// Decodes each child message with the converter of the member it was encoded from. The encoder
    /// pairs a key with its member value, so a lookup table that maps a key to the wrong member
    /// index makes this fail even though encode and decode would otherwise agree with each other.
    /// </summary>
    private static void ValidateChildren(IGenerator generator, RandomizedType plan, object item, byte[] wire)
    {
        var token = new Token(generator, wire);
        Assert.Equal(plan.Members.Select(x => x.Key).ToHashSet(StringComparer.Ordinal), token.Children.Keys.ToHashSet(StringComparer.Ordinal));
        foreach (var member in plan.Members)
        {
            var memberConverter = generator.GetConverter(RandomizedNamedObjectSource.Resolve(member.TypeName));
            var value = memberConverter.Decode(token.Children[member.Key].Memory.Span);
            Assert.Equal(GetValue(item, member), value);
        }
    }

    /// <summary>
    /// Drops optional members one at a time. Required members keep their slots, so a missing
    /// optional member shifts every following entry and the slice table has to stay consistent.
    /// </summary>
    private static void ValidateOptionalMembers(Type type, RandomizedType plan, IConverter converter, IGenerator generator)
    {
        var members = plan.Members;
        var optional = members.Where(x => !x.IsRequired).ToList();
        if (optional.Count is 0)
            return;
        var requiredKeys = members.Where(x => x.IsRequired).Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
        for (var i = 0; i < optional.Count; i++)
        {
            var member = optional[i];
            var single = Activator.CreateInstance(type)!;
            SetValue(single, member, CreateValue(member, i + 1));
            var wire = converter.Encode(single);
            var token = new Token(generator, wire);
            var expected = new HashSet<string>(requiredKeys, StringComparer.Ordinal) { member.Key };
            Assert.Equal(expected, token.Children.Keys.ToHashSet(StringComparer.Ordinal));
            var decoded = converter.Decode(wire)!;
            foreach (var other in optional)
                Assert.Equal(GetValue(single, other), GetValue(decoded, other));
        }
    }

    private static void ValidateReordered(IConverter converter, IGenerator generator, object item, RandomizedType plan, byte[] wire)
    {
        var keys = plan.Members.Select(x => x.Key).Reverse().ToList();
        var reordered = Rebuild(generator, wire, keys);
        var decoded = converter.Decode(reordered)!;
        foreach (var member in plan.Members)
            Assert.Equal(GetValue(item, member), GetValue(decoded, member));
    }

    private static void ValidateMissingKey(IConverter converter, IGenerator generator, RandomizedType plan, byte[] wire)
    {
        var members = plan.Members;
        if (members.Count < 2)
            return;
        var missing = members.FirstOrDefault(x => x.IsRequired);
        if (missing is null)
            return;
        var keys = members.Where(x => !ReferenceEquals(x, missing)).Select(x => x.Key).ToList();
        var incomplete = Rebuild(generator, wire, keys);
        var error = Assert.Throws<ArgumentException>(() => _ = converter.Decode(incomplete));
        Assert.Contains(missing.Key, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Appends a key the lookup table has to reject: a real key with a single byte changed, so the
    /// length and the first byte still match. A table that compares only the leading word of a nine
    /// to fifteen byte key, or that stops before the trailing byte of a three byte key, still matches
    /// every exact key; only such a near miss resolves to the member it was derived from, which turns
    /// into the wrong value or a duplicated key error.
    /// </summary>
    private static void ValidateNearMissKeys(IConverter converter, IGenerator generator, RandomizedType plan, byte[] wire, object expected)
    {
        var members = plan.Members;
        // keys shorter than three bytes have no position that is neither the first nor the last one
        var member = members.FirstOrDefault(x => Encoding.UTF8.GetByteCount(x.Key) >= 3);
        if (member is null)
            return;
        var key = Encoding.UTF8.GetBytes(member.Key);
        var siblings = members.Select(x => Encoding.UTF8.GetBytes(x.Key)).Where(x => x.Length == key.Length).ToArray();
        // the middle byte catches a comparison that stops after the leading word, the last byte
        // catches one that stops before the trailing byte of a three byte key
        var extras = new List<byte[]>(2);
        foreach (var index in new HashSet<int> { key.Length / 2, key.Length - 1 })
        {
            // a byte no sibling key of the same length carries at this position, so the near miss can
            // never collide with another member
            var taken = siblings.Select(x => x[index]).ToHashSet();
            var near = (byte[])key.Clone();
            near[index] = Enumerable.Range(0, 256).Select(x => (byte)x).First(x => x != key[index] && taken.Contains(x) is false);
            extras.Add(near);
        }
        AssertIgnoredKeys(converter, generator, plan, wire, expected, [.. extras]);
    }

    private static void ValidateUnknownKey(IConverter converter, IGenerator generator, RandomizedType plan, byte[] wire, object expected)
        => AssertIgnoredKeys(converter, generator, plan, wire, expected, "unknown_key_zzz"u8.ToArray());

    /// <summary>
    /// Rebuilds the wire with the given keys prepended and checks the decoder skipped all of them.
    /// </summary>
    private static void AssertIgnoredKeys(IConverter converter, IGenerator generator, RandomizedType plan, byte[] wire, object expected, params byte[][] extras)
    {
        var token = new Token(generator, wire);
        var allocator = new Allocator();
        foreach (var extra in extras)
        {
            Converter.EncodeWithLengthPrefix(ref allocator, extra);
            Converter.EncodeWithLengthPrefix(ref allocator, BitConverter.GetBytes(12345));
        }
        foreach (var member in plan.Members)
        {
            Converter.EncodeWithLengthPrefix(ref allocator, Encoding.UTF8.GetBytes(member.Key));
            Converter.EncodeWithLengthPrefix(ref allocator, token.Children[member.Key].Memory.Span);
        }
        var decoded = converter.Decode(allocator.ToArray())!;
        foreach (var member in plan.Members)
            Assert.Equal(GetValue(expected, member), GetValue(decoded, member));
    }

    private static byte[] Rebuild(IGenerator generator, byte[] wire, IEnumerable<string> keys)
    {
        var token = new Token(generator, wire);
        var allocator = new Allocator();
        foreach (var key in keys)
        {
            Converter.EncodeWithLengthPrefix(ref allocator, Encoding.UTF8.GetBytes(key));
            Converter.EncodeWithLengthPrefix(ref allocator, token.Children[key].Memory.Span);
        }
        return allocator.ToArray();
    }

    private static object CreateValue(RandomizedMember member, int index) => member.TypeName switch
    {
        "int" => index + 1,
        "long" => (index + 1) * 1000L,
        "double" => index + 0.5,
        "string" => $"value-{index}",
        _ => throw new NotSupportedException($"Unsupported member type: {member.TypeName}"),
    };

    private static void SetValue(object item, RandomizedMember member, object value)
    {
        var type = item.GetType();
        if (member.IsField)
            type.GetField(member.Name)!.SetValue(item, value);
        else
            type.GetProperty(member.Name)!.SetValue(item, value);
    }

    private static object? GetValue(object item, RandomizedMember member)
    {
        var type = item.GetType();
        return member.IsField
            ? type.GetField(member.Name)!.GetValue(item)
            : type.GetProperty(member.Name)!.GetValue(item);
    }
}
