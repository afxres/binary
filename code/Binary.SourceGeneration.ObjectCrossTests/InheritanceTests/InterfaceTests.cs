namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.InheritanceTests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

interface ISameSignatureA
{
    string Name { get; }
}

interface ISameSignatureB
{
    string Name { get; }
}

interface ISameSignatureChild : ISameSignatureA, ISameSignatureB { }

interface IDifferentSetterA
{
    string Key { get; }
}

interface IDifferentSetterB
{
    string Key { get; set; }
}

interface IDifferentSetterChild : IDifferentSetterA, IDifferentSetterB { }

interface IDifferentTypeA
{
    int Id { get; }
}

interface IDifferentTypeB
{
    string Id { get; }
}

interface IDifferentTypeChild : IDifferentTypeA, IDifferentTypeB { }

interface IDeepABase
{
    string Item { get; }
}

interface IDeepBBase
{
    string Item { get; }
}

interface IDeepA : IDeepABase { }

interface IDeepB : IDeepBBase { }

interface IDeepChild : IDeepA, IDeepB { }

interface IMultipleA
{
    int A { get; }

    double B { get; }

    string C { get; }
}

interface IMultipleB
{
    int B { get; }

    string C { get; }

    Guid D { get; }
}

interface IMultipleChild : IMultipleA, IMultipleB { }

public class InterfaceTests
{
    public static IEnumerable<object[]> InterfaceAmbiguousData()
    {
        yield return new object[] { typeof(ISameSignatureChild), new[] { "Name" } };
        yield return new object[] { typeof(IDifferentSetterChild), new[] { "Key" } };
        yield return new object[] { typeof(IDifferentTypeChild), new[] { "Id" } };
        yield return new object[] { typeof(IDeepChild), new[] { "Item" } };
        yield return new object[] { typeof(IMultipleChild), new[] { "B", "C" } };
        yield return new object[] { typeof(IShadowingE), new[] { "Member" } };
        yield return new object[] { typeof(IMessA3), new[] { "A" } };
    }

    [Theory(DisplayName = "Interface Ambiguous Test")]
    [MemberData(nameof(InterfaceAmbiguousData))]
    public void InterfaceAmbiguousTest(Type wanted, IEnumerable<string> names)
    {
        Assert.NotNull(wanted);
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "GetAllFieldsAndProperties");
        var reflectionFunction = (Func<Type, BindingFlags, ImmutableArray<MemberInfo>>)Delegate.CreateDelegate(typeof(Func<Type, BindingFlags, ImmutableArray<MemberInfo>>), reflectionMethod);

        var fullName = wanted.FullName;
        Assert.NotNull(fullName);
        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbol = compilation.GetTypeByMetadataName(fullName);
        Assert.NotNull(symbol);

        var symbolResult = Symbols.GetAllFieldsAndProperties(compilation, symbol, out var conflict, default);
        Assert.Empty(symbolResult);
        Assert.Equal(names, conflict);

        var error = Assert.Throws<ArgumentException>(() => reflectionFunction.Invoke(wanted, BindingFlags.Instance | BindingFlags.Public));
        var expected = new Regex("Get members error, ambiguous members detected, member name: (\\w*), type: (\\S*)");
        var matches = expected.Matches(error.Message);
        var match = Assert.Single(matches);
        Assert.Equal(3, match.Groups.Count);
        Assert.Contains(match.Groups[1].Value, names);
        Assert.Equal(wanted.FullName, match.Groups[2].Value);
    }
}
