namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.RequiredMembers;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<FieldOnlyItem>]
[SourceGeneratorInclude<PropertyOnlyItem>]
public partial class RequiredMembersSourceGeneratorContext { }

[NamedObject]
public class FieldOnlyItem : IEquatable<FieldOnlyItem?>
{
    [NamedKey("alpha")]
    public required int Required;

    [NamedKey("bravo")]
    public string? Optional;

    public bool Equals(FieldOnlyItem? other) => other is not null && this.Required == other.Required && this.Optional == other.Optional;

    public override bool Equals(object? obj) => Equals(obj as FieldOnlyItem);

    public override int GetHashCode() => HashCode.Combine(this.Required, this.Optional);
}

[NamedObject]
public class PropertyOnlyItem : IEquatable<PropertyOnlyItem?>
{
    [NamedKey("required")]
    public required int RequiredMember { get; init; }

    [NamedKey("optional")]
    public string? OptionalMember { get; init; }

    public bool Equals(PropertyOnlyItem? other) => other is not null && RequiredMember == other.RequiredMember && OptionalMember == other.OptionalMember;

    public override bool Equals(object? obj) => Equals(obj as PropertyOnlyItem);

    public override int GetHashCode() => HashCode.Combine(RequiredMember, OptionalMember);
}

public class RequiredMembersTests
{
    public static IEnumerable<object[]> FieldOnlyAllMemberSetData()
    {
        var keys = new[] { "alpha", "bravo" };
        yield return new object[] { keys, new FieldOnlyItem { Required = 5, Optional = "6" } };
        yield return new object[] { keys, new FieldOnlyItem { Required = 7, Optional = "8" } };
    }

    public static IEnumerable<object[]> FieldOnlyRequiredMemberSetData()
    {
        var keys = new[] { "alpha" };
        yield return new object[] { keys, new FieldOnlyItem { Required = 10 } };
        yield return new object[] { keys, new FieldOnlyItem { Required = 11 } };
    }

    public static IEnumerable<object[]> PropertyOnlyAllMemberSetData()
    {
        var keys = new[] { "required", "optional" };
        yield return new object[] { keys, new PropertyOnlyItem { RequiredMember = 1, OptionalMember = "1" } };
        yield return new object[] { keys, new PropertyOnlyItem { RequiredMember = 2, OptionalMember = "Second" } };
    }

    public static IEnumerable<object[]> PropertyOnlyRequiredMemberSetData()
    {
        var keys = new[] { "required" };
        yield return new object[] { keys, new PropertyOnlyItem { RequiredMember = 255 } };
        yield return new object[] { keys, new PropertyOnlyItem { RequiredMember = 256 } };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(FieldOnlyAllMemberSetData))]
    [MemberData(nameof(PropertyOnlyAllMemberSetData))]
    [MemberData(nameof(FieldOnlyRequiredMemberSetData))]
    [MemberData(nameof(PropertyOnlyRequiredMemberSetData))]
    public void EncodeDecodeTest<T>(IEnumerable<string> expectedKeys, T source)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(RequiredMembersSourceGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal(converterType.Assembly, typeof(RequiredMembersSourceGeneratorContext).Assembly);

        var buffer = converter.Encode(source);
        var token = new Token(generator, buffer);
        Assert.Equal(expectedKeys.ToHashSet(), token.Children.Keys.ToHashSet());

        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
    }
}
