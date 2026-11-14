#if NET11_0_OR_GREATER

namespace Mikodev.Binary.Tests.Creators;

using Mikodev.Binary.Attributes;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

// public, because the synthesized union members are public and take these types as parameters
public readonly record struct BareEmptyStruct;

public sealed class BareEmptyClass { }

public sealed record BareEmptyRecord;

public readonly union UnionWithEmptyStructCase(BareEmptyStruct, int);

public readonly union UnionWithEmptyClassCase(BareEmptyClass, int);

public readonly union UnionWithEmptyRecordCase(BareEmptyRecord, int);

public class UnionEmptyCaseTests
{
    // Stands in for a union type: IsEmptyUnionCaseType only reads the declaring assembly.
    private sealed class UnionType;

    private readonly struct EmptyStruct { }

    private sealed class EmptyClass { }

    private readonly record struct EmptyRecordStruct;

    private record RecordClass;

    private sealed record SealedRecordClass;

    private sealed record RecordClassWithValue(int Value);

    private readonly struct StructWithPublicField
    {
        public readonly int Value;

        public StructWithPublicField(int value) => Value = value;
    }

    private sealed class ClassWithPublicProperty
    {
        public int Value { get; set; }
    }

    private readonly struct StructWithPrivateField
    {
        private readonly int value;

        public StructWithPrivateField(int value) => this.value = value;

        // only keeps the field reachable for the analyzer; a method is not state
        public int Read() => this.value;
    }

    private sealed class ClassWithUserDeclaredEqualityContract
    {
        public Type EqualityContract => typeof(ClassWithUserDeclaredEqualityContract);
    }

    [NamedObject]
    private sealed class AttributedEmptyClass { }

    private sealed class ClassWithoutPublicParameterlessConstructor
    {
        public ClassWithoutPublicParameterlessConstructor(int value) => _ = value;
    }

    private abstract class AbstractCase { }

    private interface IEmptyInterface { }

    private static readonly MethodInfo IsEmptyUnionCaseTypeMethod = typeof(Generator).Assembly
        .GetType("Mikodev.Binary.Creators.UnionConverterCreator", throwOnError: true)!
        .GetMethod("IsEmptyUnionCaseType", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static bool IsEmpty(Type caseType)
    {
        var result = IsEmptyUnionCaseTypeMethod.Invoke(null, [typeof(UnionType), caseType]);
        return Assert.IsType<bool>(result);
    }

    [Fact(DisplayName = "Empty Types In Same Assembly Test")]
    public void EmptyTypesInSameAssemblyAreEmptyCases()
    {
        Assert.True(IsEmpty(typeof(EmptyStruct)));
        Assert.True(IsEmpty(typeof(EmptyClass)));
        Assert.True(IsEmpty(typeof(EmptyRecordStruct)));
    }

    [Fact(DisplayName = "Record Classes With Only Equality Contract Test")]
    public void RecordClassesWithOnlyEqualityContractAreEmptyCases()
    {
        // 'record class' always synthesizes an 'EqualityContract' property, even without any field,
        // because it answers 'typeof(T)' and therefore carries no per instance state. 'record struct'
        // does not get one at all.
        Assert.True(IsEmpty(typeof(RecordClass)));
        Assert.True(IsEmpty(typeof(SealedRecordClass)));
    }

    [Fact(DisplayName = "Record Classes With State Test")]
    public void RecordClassesWithStateAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(RecordClassWithValue)));
    }

    [Fact(DisplayName = "User Declared Equality Contract Test")]
    public void UserDeclaredEqualityContractIsNotEmptyCase()
    {
        // only the compiler synthesized property is metadata, a hand written one is ordinary state
        Assert.False(IsEmpty(typeof(ClassWithUserDeclaredEqualityContract)));
    }

    [Fact(DisplayName = "Value Tuple Test")]
    public void ValueTupleIsAnEmptyCase()
    {
        Assert.True(IsEmpty(typeof(ValueTuple)));
    }

    [Fact(DisplayName = "Types From Another Assembly Test")]
    public void TypesFromAnotherAssemblyAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(int)));
        Assert.False(IsEmpty(typeof(decimal)));
        Assert.False(IsEmpty(typeof(Guid)));
        Assert.False(IsEmpty(typeof(object)));
        Assert.False(IsEmpty(typeof(string)));
        Assert.False(IsEmpty(typeof(DateTime)));
    }

    [Fact(DisplayName = "Types With Public Members Test")]
    public void TypesWithPublicMembersAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(StructWithPublicField)));
        Assert.False(IsEmpty(typeof(ClassWithPublicProperty)));
    }

    [Fact(DisplayName = "Attributed Types Test")]
    public void AttributedTypesAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(AttributedEmptyClass)));
    }

    [Fact(DisplayName = "Abstract And Interface Types Test")]
    public void AbstractAndInterfaceTypesAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(AbstractCase)));
        Assert.False(IsEmpty(typeof(IEmptyInterface)));
    }

    [Fact(DisplayName = "Types Without Public Parameterless Constructor Test")]
    public void TypesWithoutPublicParameterlessConstructorAreNotEmptyCases()
    {
        Assert.False(IsEmpty(typeof(ClassWithoutPublicParameterlessConstructor)));
    }

    [Fact(DisplayName = "Private Fields Test")]
    public void PrivateFieldsCountAsState()
    {
        // Non public members are counted too, so a type that hides its state behind a private field
        // is not mistaken for an empty case. This is what keeps int out of the branch as well, even
        // without the assembly guard.
        Assert.False(IsEmpty(typeof(StructWithPrivateField)));
    }

    private static void AssertEmptyCaseWire<TUnion>(TUnion source, Type expectedEmptyType)
    {
        var converter = Generator.CreateDefault().GetConverter<TUnion>();
        Assert.Equal(0, converter.Length);

        var buffer = converter.Encode(source);
        Assert.Equal("00", Convert.ToHexString(buffer));

        var autoBuffer = Allocator.Invoke(source, converter.EncodeAuto);
        Assert.Equal("00", Convert.ToHexString(autoBuffer));

        var span = new ReadOnlySpan<byte>(buffer);
        var result = converter.DecodeAuto(ref span);
        Assert.Equal(0, span.Length);
        Assert.IsType(expectedEmptyType, ((IUnion)(object)result!).Value);
    }

    private static void AssertNonEmptyCaseWire<TUnion>(TUnion source)
    {
        var converter = Generator.CreateDefault().GetConverter<TUnion>();
        var buffer = converter.Encode(source);
        Assert.Equal("012A000000", Convert.ToHexString(buffer));
        Assert.Equal(source, converter.Decode(buffer));
    }

    [Fact(DisplayName = "Union With Empty Case Wire Test")]
    public void UnionWithEmptyCaseWireTest()
    {
        AssertEmptyCaseWire(new UnionWithEmptyStructCase(default(BareEmptyStruct)), typeof(BareEmptyStruct));
        AssertEmptyCaseWire(new UnionWithEmptyClassCase(new BareEmptyClass()), typeof(BareEmptyClass));
        AssertEmptyCaseWire(new UnionWithEmptyRecordCase(new BareEmptyRecord()), typeof(BareEmptyRecord));
    }

    [Fact(DisplayName = "Union With Empty Case And Non Empty Case Wire Test")]
    public void UnionWithEmptyCaseAndNonEmptyCaseWireTest()
    {
        AssertNonEmptyCaseWire(new UnionWithEmptyStructCase(0x2A));
        AssertNonEmptyCaseWire(new UnionWithEmptyClassCase(0x2A));
        AssertNonEmptyCaseWire(new UnionWithEmptyRecordCase(0x2A));
    }

    [Fact(DisplayName = "Union Wire Compared With Payload Wire Test")]
    public void UnionWireComparedWithPayloadWireTest()
    {
        // A union is the tag byte followed by the payload wire, and the empty case contributes no
        // payload at all. The F# suite asserts the same bytes for an equivalent union, so a value
        // has one wire format regardless of the language that declared the union.
        var generator = Generator.CreateDefault();
        var unionConverter = generator.GetConverter<UnionWithEmptyStructCase>();
        var payloadWire = generator.GetConverter<int>().Encode(0x2A);
        Assert.Equal<byte>([0x2A, 0x00, 0x00, 0x00], payloadWire);

        var emptyWire = unionConverter.Encode(new UnionWithEmptyStructCase(default(BareEmptyStruct)));
        Assert.Equal<byte>([0x00], emptyWire);

        var payloadUnionWire = unionConverter.Encode(new UnionWithEmptyStructCase(0x2A));
        Assert.Equal<byte>([0x01, 0x2A, 0x00, 0x00, 0x00], payloadUnionWire);
        Assert.Equal<byte>(payloadWire, payloadUnionWire[1..]);
    }
}

#endif
