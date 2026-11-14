namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.IntegrationTests;

using System;
using System.Runtime.CompilerServices;
using Xunit;

public interface IIntCaseFactory
{
    static abstract InheritedUnionCaseUnion Create(int value);
}

// a case factory inherited from a base interface is not supported, the reflection based
// creator reports it instead of silently dropping the case, and the source generator
// reports 'BINSRCGEN72' for the same shape
[Union]
public sealed class InheritedUnionCaseUnion : InheritedUnionCaseUnion.IUnionMembers, IIntCaseFactory
{
    private readonly object? value;

    private InheritedUnionCaseUnion(object? value) => this.value = value;

    public interface IUnionMembers : IIntCaseFactory
    {
        static InheritedUnionCaseUnion Create(string value) => new(value);

        object? Value { get; }
    }

    static InheritedUnionCaseUnion IIntCaseFactory.Create(int value) => new(value);

    object? IUnionMembers.Value => this.value;
}

public class UnionInheritedCaseTests
{
    [Fact(DisplayName = "Inherited Union Case Factory Test")]
    public void InheritedUnionCaseFactoryTest()
    {
        var type = typeof(InheritedUnionCaseUnion);
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(type));
        Assert.Null(error.ParamName);
        Assert.Equal($"Union case factory is inherited, type: {type}", error.Message);
    }
}
