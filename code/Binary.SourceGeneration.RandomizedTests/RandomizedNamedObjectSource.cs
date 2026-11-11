namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using System;
using System.Collections.Generic;
using System.Text;

internal static class RandomizedNamedObjectSource
{
    private static readonly string[] SimpleTypes = ["int", "long", "double", "string"];

    /// <summary>How many keys every cluster holds.</summary>
    private const int ClusterCount = 3;

    public static string Create(int seed, int typeCount, out IReadOnlyList<RandomizedType> types)
    {
        var random = new Random(seed);
        var builder = new StringBuilder();
        var results = new List<RandomizedType>();
        _ = builder.AppendLine("namespace Randomized;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("using Mikodev.Binary.Attributes;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("[SourceGeneratorContext]");

        for (var t = 0; t < typeCount; t++)
        {
            // the first two types pin both kinds, the rest stay random
            var kind = t is 0 ? RandomizedObjectKind.Plain
                : t is 1 ? RandomizedObjectKind.Named
                : random.Next(2) is 0 ? RandomizedObjectKind.Plain : RandomizedObjectKind.Named;
            var count = random.Next(1, 25);
            var plain = kind is RandomizedObjectKind.Plain;
            var used = new HashSet<string>(StringComparer.Ordinal);
            string? previous = null;
            var keys = new List<string>(count + ClusterCount);
            for (var i = 0; i < count; i++)
                keys.Add(RandomKeys.Next(random, ref previous, used, startsUpperCase: plain));
            // every type gets three clusters of three keys each. Inside a cluster the keys share their
            // length, their first byte and every byte but the last one, so the lookup table has to
            // walk a comparison chain instead of matching in one shot. The long group also switches
            // the wire to the four byte length prefix, the short group sits around the vector inline
            // limit, and the multibyte group decouples the byte length the table buckets by from the
            // character count.
            keys.AddRange(RandomKeys.LongCluster(random, ClusterCount, used, startsUpperCase: plain));
            keys.AddRange(RandomKeys.ShortCluster(random, ClusterCount, used, startsUpperCase: plain));
            keys.AddRange(RandomKeys.MultibyteCluster(random, ClusterCount, used));
            var members = new List<RandomizedMember>(keys.Count);
            for (var i = 0; i < keys.Count; i++)
            {
                members.Add(new RandomizedMember
                {
                    Name = plain ? keys[i] : $"M{i:D3}",
                    Key = keys[i],
                    TypeName = SimpleTypes[random.Next(SimpleTypes.Length)],
                    // at least one required member keeps the missing key check on the generated path
                    IsRequired = i is 0 || random.Next(2) is 0,
                    IsField = random.Next(2) is 0,
                    TupleIndex = 0,
                });
            }
            var type = new RandomizedType
            {
                Name = $"Item{t}",
                Kind = kind,
                IsStruct = random.Next(4) is 0,
                Members = members,
            };
            results.Add(type);
            _ = builder.AppendLine($"[SourceGeneratorInclude<{type.Name}>]");
        }
        _ = builder.AppendLine("public partial class RandomizedGeneratorContext { }");

        foreach (var type in results)
        {
            _ = builder.AppendLine();
            if (type.Kind is RandomizedObjectKind.Named)
                _ = builder.AppendLine("[NamedObject]");
            _ = builder.AppendLine($"public {(type.IsStruct ? "struct" : "class")} {type.Name}");
            _ = builder.AppendLine("{");
            foreach (var member in type.Members)
            {
                if (type.Kind is RandomizedObjectKind.Named)
                    _ = builder.AppendLine($"    [NamedKey(\"{member.Key}\")]");
                var required = member.IsRequired ? "required " : string.Empty;
                _ = builder.AppendLine(member.IsField
                    ? $"    public {required}{member.TypeName} {member.Name};"
                    : $"    public {required}{member.TypeName} {member.Name} {{ get; set; }}");
                _ = builder.AppendLine();
            }
            _ = builder.AppendLine("}");
        }
        types = results;
        return builder.ToString();
    }

    /// <summary>Resolves the primitive member types written into the source.</summary>
    public static Type Resolve(string typeName) => typeName switch
    {
        "int" => typeof(int),
        "long" => typeof(long),
        "double" => typeof(double),
        "string" => typeof(string),
        _ => throw new NotSupportedException($"Unsupported member type: {typeName}"),
    };
}
