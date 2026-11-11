namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using Mikodev.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

/// <summary>
/// Compiles randomly shaped tuple object types at run time. Every type is checked against the
/// reflection converter, against a value tuple with the same logical member order, and against a
/// value tuple with the embedded tuple objects flattened in place.
/// </summary>
public class RandomizedTupleObjectTests
{
    private const int TypeCount = 4;

    [Theory(DisplayName = "Randomized Tuple Object Test")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void RandomizedTupleObjectTest(int seed)
    {
        var source = RandomizedTupleObjectSource.Create(seed, TypeCount, out var types);
        Assert.Contains(types, x => x.Members.Any(y => y.IsNested));
        using var assembly = DynamicAssembly.Create(source, $"RandomizedTupleObject{seed}");
        var creators = assembly.GetConverterCreators("Randomized.RandomizedGeneratorContext");
        var generated = Generator.CreateAotBuilder().AddConverterCreators(creators.Values).Build();
        var reflected = Generator.CreateDefault();
        foreach (var plan in types)
            Validate(assembly, plan, generated, reflected);
    }

    private static void Validate(DynamicAssembly assembly, RandomizedType plan, IGenerator generated, IGenerator reflected)
    {
        var type = assembly.GetType(plan.Name);
        var converter = generated.GetConverter(type);
        var reference = reflected.GetConverter(type);
        Assert.Equal(reference.Length, converter.Length);

        var cursor = 0;
        var item = CreateObject(assembly, type, plan, ref cursor);
        var wire = converter.Encode(item);
        Assert.Equal(reference.Encode(item), wire);

        var decoded = converter.Decode(wire)!;
        AssertMembersEqual(item, decoded, plan);
        var decodedByReference = reference.Decode(wire)!;
        AssertMembersEqual(item, decodedByReference, plan);

        // the same shape written as a value tuple, embedded tuple objects stay nested
        cursor = 0;
        var nestedTuple = CreateNestedTuple(assembly, plan, ref cursor);
        var nestedType = nestedTuple.GetType();
        var nestedConverter = generated.GetConverter(nestedType);
        var nestedWire = nestedConverter.Encode(nestedTuple);
        Assert.Equal(wire, nestedWire);
        Assert.Equal(nestedTuple, nestedConverter.Decode(wire));

        // the flattened shape, embedded tuple objects expanded in place
        if (RandomizedTupleObjectSource.FlatSyntax(plan) != RandomizedTupleObjectSource.TupleSyntax(plan))
        {
            cursor = 0;
            var flatTuple = CreateFlatTuple(plan, ref cursor);
            var flatType = flatTuple.GetType();
            var flatConverter = generated.GetConverter(flatType);
            var flatWire = flatConverter.Encode(flatTuple);
            Assert.Equal(wire, flatWire);
            Assert.Equal(flatTuple, flatConverter.Decode(wire));
        }
    }

    private static object CreateObject(DynamicAssembly assembly, Type type, RandomizedType plan, ref int cursor)
    {
        var values = new Dictionary<RandomizedMember, object>();
        foreach (var member in RandomizedTupleObjectSource.Ordered(plan))
            values[member] = member.IsNested
                ? CreateObject(assembly, assembly.GetType(member.TypeName), member.Nested!, ref cursor)
                : CreatePrimitive(member.TypeName, ref cursor);
        var instance = Activator.CreateInstance(type)!;
        foreach (var member in plan.Members)
            type.GetProperty(member.Name)!.SetValue(instance, values[member]);
        return instance;
    }

    private static object CreateNestedTuple(DynamicAssembly assembly, RandomizedType plan, ref int cursor)
    {
        var items = new List<object>();
        foreach (var member in RandomizedTupleObjectSource.Ordered(plan))
            items.Add(member.IsNested ? CreateNestedTuple(assembly, member.Nested!, ref cursor) : CreatePrimitive(member.TypeName, ref cursor));
        return CreateTuple(items);
    }

    private static object CreateFlatTuple(RandomizedType plan, ref int cursor)
    {
        var items = new List<object>();
        CollectFlat(plan, items, ref cursor);
        return CreateTuple(items);
    }

    private static void CollectFlat(RandomizedType plan, List<object> items, ref int cursor)
    {
        foreach (var member in RandomizedTupleObjectSource.Ordered(plan))
        {
            if (member.IsNested)
                CollectFlat(member.Nested!, items, ref cursor);
            else
                items.Add(CreatePrimitive(member.TypeName, ref cursor));
        }
    }

    private static object CreateTuple(List<object> items)
    {
        var types = items.Select(x => x.GetType()).ToArray();
        var definition = typeof(ValueTuple<,>).Assembly.GetType($"System.ValueTuple`{types.Length}", throwOnError: true)!;
        return Activator.CreateInstance(definition.MakeGenericType(types), [.. items])!;
    }

    private static object CreatePrimitive(string typeName, ref int cursor)
    {
        var index = cursor++;
        return typeName switch
        {
            "int" => index + 1,
            "long" => (index + 1) * 1000L,
            "double" => index + 0.5,
            "string" => $"value-{index}",
            _ => throw new NotSupportedException($"Unsupported member type: {typeName}"),
        };
    }

    private static void AssertMembersEqual(object expected, object actual, RandomizedType plan)
    {
        foreach (var member in plan.Members)
        {
            var left = GetValue(expected, member);
            var right = GetValue(actual, member);
            if (member.IsNested)
                AssertMembersEqual(left!, right!, member.Nested!);
            else
                Assert.Equal(left, right);
        }
    }

    private static object? GetValue(object item, RandomizedMember member) => item.GetType().GetProperty(member.Name)!.GetValue(item);
}
