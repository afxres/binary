namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.Attributes;
using System.Collections.Generic;
using Xunit;

public class RequireKeyAttributeTests
{
    public static IEnumerable<object[]> ConverterAttributeData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class TestGeneratorContext { }

            class FakeConverter<T> : Converter<T>
            {
                public override T Decode(in ReadOnlySpan<byte> span) => throw new NotImplementedException();

                public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();
            }

            class Alpha
            {
                [Converter(typeof(FakeConverter<int>))]
                public int Id { get; set; }
            }
            """;
        yield return [a, "Id", "Alpha"];
    }

    [Theory(DisplayName = "Require Key Attribute For Converter Attribute Test")]
    [MemberData(nameof(ConverterAttributeData))]
    public void RequireKeyAttributeForConverterAttributeTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{nameof(ConverterAttribute)}', member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Matches(@"Converter\(.*\)", diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> ConverterCreatorAttributeData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary;
            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            partial class TestGeneratorContext { }

            class FakeConverterCreator : IConverterCreator
            {
                public IConverter? GetConverter(IGeneratorContext context, Type type) => throw new NotImplementedException();
            }

            class Delta
            {
                [ConverterCreator(typeof(FakeConverterCreator))]
                public string Name;
            }
            """;
        yield return [a, "Name", "Delta"];
    }

    [Theory(DisplayName = "Require Key Attribute For Converter Creator Attribute Test")]
    [MemberData(nameof(ConverterCreatorAttributeData))]
    public void RequireKeyAttributeForConverterCreatorAttributeTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{nameof(ConverterCreatorAttribute)}', member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Matches(@"ConverterCreator\(.*\)", diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> RequiredMemberData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class TestGeneratorContext { }

            [NamedObject]
            class Alpha
            {
                [NamedKey("first")]
                public required int First { get; set; }

                public required string Second { get; set; }
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class TestGeneratorContext { }

            [TupleObject]
            class Bravo
            {
                public required int Head;

                [TupleKey(0)]
                public required string Tail;
            }
            """;
        yield return [a, "Second", "Alpha", "Named"];
        yield return [b, "Head", "Bravo", "Tuple"];
    }

    public static IEnumerable<object[]> RequiredMemberBaseTypeData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Self>]
            partial class TestGeneratorContext { }

            class Base
            {
                public required int BaseMember;
            }

            [NamedObject]
            class Self : Base
            {
                [NamedKey("member")]
                public required int Member;
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Student>]
            partial class TestGeneratorContext { }

            class Person
            {
                public required string Name { get; set; }
            }

            [TupleObject]
            class Student : Person
            {
                [TupleKey(0)]
                public required int Id { get; set; }
            }
            """;
        yield return [a, "BaseMember", "Self", "Named"];
        yield return [b, "Name", "Student", "Tuple"];
    }

    [Theory(DisplayName = "Require Key Attribute For Required Member Test")]
    [MemberData(nameof(RequiredMemberData))]
    [MemberData(nameof(RequiredMemberBaseTypeData))]
    public void RequireKeyAttributeForRequiredMemberTest(string source, string memberName, string typeName, string typeKind)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal($"Require {typeKind} Key Attribute For Required Member.", diagnostic.Descriptor.Title);
        Assert.EndsWith($"Require '{typeKind}KeyAttribute' for required member, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Equal(memberName, diagnostic.Location.GetSourceText());
    }
}
