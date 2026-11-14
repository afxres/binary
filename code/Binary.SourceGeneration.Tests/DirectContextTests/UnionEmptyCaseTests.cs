#if NET11_0_OR_GREATER

namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Xunit;

public class UnionEmptyCaseTests
{
    private const string UnionTypeName = "UnionTests.Sample";

    private const string Source =
        """
        namespace UnionTests;

        using Mikodev.Binary.Attributes;

        public readonly record struct EmptyStruct;

        public sealed class EmptyClass { }

        public sealed record EmptyRecord;

        public sealed record RecordWithValue(int Value);

        public readonly struct StructWithPublicField
        {
            public readonly int Value;

            public StructWithPublicField(int value) => Value = value;
        }

        public readonly struct StructWithPrivateField
        {
            private readonly int value;

            public StructWithPrivateField(int value) => this.value = value;

            public int Read() => this.value;
        }

        public sealed class ClassWithEqualityContract
        {
            public System.Type EqualityContract => typeof(ClassWithEqualityContract);
        }

        [NamedObject]
        public sealed class AttributedEmptyClass { }

        public sealed class ClassWithoutParameterlessConstructor
        {
            public ClassWithoutParameterlessConstructor(int value) => _ = value;
        }

        public abstract class AbstractCase { }

        public interface IEmptyInterface { }

        readonly union Sample(EmptyStruct, int);
        """;

    private static readonly Compilation TestCompilation = CompilationModule.CreateCompilation(Source);

    private static readonly MethodInfo IsEmptyUnionCaseTypeMethod = typeof(UnionObjectConverterContext)
        .GetMethod("IsEmptyUnionCaseType", BindingFlags.Static | BindingFlags.NonPublic)!;

    public static IEnumerable<object[]> EmptyCaseTypeData()
    {
        yield return ["UnionTests.EmptyStruct", true];
        yield return ["UnionTests.EmptyClass", true];
        yield return ["UnionTests.EmptyRecord", true];
        yield return ["UnionTests.RecordWithValue", false];
        yield return ["UnionTests.StructWithPublicField", false];
        yield return ["UnionTests.StructWithPrivateField", false];
        yield return ["UnionTests.ClassWithEqualityContract", false];
        yield return ["UnionTests.AttributedEmptyClass", false];
        yield return ["UnionTests.ClassWithoutParameterlessConstructor", false];
        yield return ["UnionTests.AbstractCase", false];
        yield return ["UnionTests.IEmptyInterface", false];
        yield return ["System.ValueTuple", true];
        yield return ["System.Int32", false];
        yield return ["System.Decimal", false];
        yield return ["System.Guid", false];
        yield return ["System.Object", false];
        yield return ["System.String", false];
        yield return ["System.DateTime", false];
    }

    [Theory(DisplayName = "Empty Union Case Type Test")]
    [MemberData(nameof(EmptyCaseTypeData))]
    public void EmptyUnionCaseTypeTest(string caseTypeMetadataName, bool expected)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var context = new SourceGeneratorContext(TestCompilation, diagnostics.Add, CancellationToken.None);
        var unionType = Assert.IsAssignableFrom<ITypeSymbol>(TestCompilation.GetTypeByMetadataName(UnionTypeName));
        var caseType = Assert.IsAssignableFrom<ITypeSymbol>(TestCompilation.GetTypeByMetadataName(caseTypeMetadataName));
        var actual = Assert.IsType<bool>(IsEmptyUnionCaseTypeMethod.Invoke(null, [context, unionType, caseType]));
        Assert.DoesNotContain(diagnostics, x => x.Severity is DiagnosticSeverity.Error);
        Assert.Equal(expected, actual);
    }
}

#endif
