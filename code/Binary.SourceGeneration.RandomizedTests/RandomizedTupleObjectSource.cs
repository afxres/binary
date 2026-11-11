namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal static class RandomizedTupleObjectSource
{
    private static readonly string[] SimpleTypes = ["int", "long", "double", "string"];

    public static IReadOnlyList<RandomizedMember> Ordered(RandomizedType type) => [.. type.Members.OrderBy(x => x.TupleIndex)];

    private static IEnumerable<string> TupleElements(RandomizedType type) => Ordered(type).Select(x => x.IsNested ? TupleSyntax(x.Nested!) : x.TypeName);

    private static IEnumerable<string> FlatElements(RandomizedType type) =>
        Ordered(type).SelectMany(x => x.IsNested ? FlatElements(x.Nested!) : [x.TypeName]);

    /// <summary>The tuple type shape that matches the logical member order, nesting included.</summary>
    public static string TupleSyntax(RandomizedType type) => $"({string.Join(", ", TupleElements(type))})";

    /// <summary>The same shape with every embedded tuple object expanded in place.</summary>
    public static string FlatSyntax(RandomizedType type) => $"({string.Join(", ", FlatElements(type))})";

    public static int FlatCount(RandomizedType type) => FlatElements(type).Count();

    public static string Create(int seed, int typeCount, out IReadOnlyList<RandomizedType> types)
    {
        var random = new Random(seed);
        var builder = new StringBuilder();
        var results = new List<RandomizedType>();
        var shapes = new HashSet<string>(StringComparer.Ordinal);
        _ = builder.AppendLine("namespace Randomized;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("using Mikodev.Binary.Attributes;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("[SourceGeneratorContext]");

        for (var t = 0; t < typeCount; t++)
        {
            var count = random.Next(2, 5);
            var nest = results.Count > 0 && count > 2 && random.Next(3) is 0 ? results[random.Next(results.Count)] : null;
            // the second type always embeds the first one, so every assembly exercises the flattened shape
            if (t is 1 && results.Count > 0)
                nest = results[0];
            // the flattened shape has to stay within the largest value tuple arity, which is seven
            if (nest is not null && count - 1 + FlatCount(nest) > 7)
                nest = null;
            var positions = Enumerable.Range(0, count).OrderBy(_ => random.Next()).ToArray();
            var members = new List<RandomizedMember>();
            for (var i = 0; i < count; i++)
            {
                var nested = nest is not null && i == count - 1;
                members.Add(new RandomizedMember
                {
                    Name = $"M{i:D3}",
                    Key = string.Empty,
                    TypeName = nested ? nest!.Name : SimpleTypes[random.Next(SimpleTypes.Length)],
                    IsRequired = true,
                    IsField = false,
                    TupleIndex = positions[i],
                    Nested = nested ? nest : null,
                });
            }
            var type = new RandomizedType
            {
                Name = $"Item{t}",
                Kind = RandomizedObjectKind.Tuple,
                IsStruct = false,
                Members = members,
            };
            results.Add(type);
            _ = builder.AppendLine($"[SourceGeneratorInclude<{type.Name}>]");
        }

        // the value tuple shapes are generated types too, the generator has to emit converters for them
        foreach (var type in results)
        {
            foreach (var shape in new[] { TupleSyntax(type), FlatSyntax(type) })
            {
                if (shapes.Add(shape))
                    _ = builder.AppendLine($"[SourceGeneratorInclude<{shape}>]");
            }
        }
        _ = builder.AppendLine("public partial class RandomizedGeneratorContext { }");

        foreach (var type in results)
        {
            _ = builder.AppendLine();
            _ = builder.AppendLine("[TupleObject]");
            _ = builder.AppendLine($"public class {type.Name}");
            _ = builder.AppendLine("{");
            foreach (var member in type.Members)
            {
                _ = builder.AppendLine($"    [TupleKey({member.TupleIndex})]");
                _ = builder.AppendLine($"    public required {member.TypeName} {member.Name} {{ get; init; }}");
                _ = builder.AppendLine();
            }
            _ = builder.AppendLine("}");
        }
        types = results;
        return builder.ToString();
    }
}
