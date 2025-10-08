namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.InheritanceTests;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

interface IShadowingA
{
    int Member { get; }

    int this[int key] { get; }
}

interface IShadowingB
{
    string Member { get; }

    string this[string key] { get; }
}

interface IShadowingC : IShadowingA, IShadowingB
{
    new double Member { get; }

    double this[double key] { get; }
}

interface IShadowingD
{
    short Member { get; }
}

interface IShadowingE : IShadowingC, IShadowingD { }

interface IShadowingF : IShadowingE
{
    new long Member { get; }
}

interface IMessA0
{
    int A { get; }
}

interface IMessA1
{
    int A { get; }
}

interface IMessA2
{
    int A { get; }
}

interface IMessA3 : IMessA0, IMessA1, IMessA2, IMessA5, IMessA6, IMessA7, IMessA8, IMessA9 { }

interface IMessA4 : IMessA3
{
    new string A { get; }
}

interface IMessA5
{
    int A { get; }
}

interface IMessA6
{
    int A { get; }
}

interface IMessA7
{
    int A { get; }
}

interface IMessA8
{
    int A { get; }
}

interface IMessA9
{
    int A { get; }
}

class CustomImplicitConversionTypeA
{
    public static implicit operator CustomImplicitConversionTypeB(CustomImplicitConversionTypeA __) => new CustomImplicitConversionTypeB();
}

class CustomImplicitConversionTypeB
{
    public static implicit operator CustomImplicitConversionTypeA(CustomImplicitConversionTypeB __) => new CustomImplicitConversionTypeA();
}

public class InterfaceShadowingTests
{
    public static IEnumerable<object[]> InterfaceShadowingData()
    {
        yield return [typeof(IMessA4), "A", typeof(string)];
        yield return [typeof(IShadowingC), "Member", typeof(double)];
        yield return [typeof(IShadowingF), "Member", typeof(long)];
    }

    [Theory(DisplayName = "Interface Shadowing Test")]
    [MemberData(nameof(InterfaceShadowingData))]
    public void InterfaceShadowingTest(Type wanted, string memberName, Type memberType)
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
        var symbolMember = Assert.IsType<IPropertySymbol>(Assert.Single(symbolResult, x => x.Name == memberName), exactMatch: false);
        var symbolUnique = symbolResult.Distinct(SymbolEqualityComparer.Default).ToList();
        Assert.Equal(memberType.Name, symbolMember.Type.Name);
        Assert.Empty(conflict);
        Assert.Equal(symbolUnique.Count, symbolResult.Length);

        var memberResult = reflectionFunction.Invoke(wanted, BindingFlags.Instance | BindingFlags.Public);
        var memberActual = Assert.IsType<PropertyInfo>(Assert.Single(memberResult, x => x.Name == memberName), exactMatch: false);
        var memberUnique = memberResult.Distinct().ToList();
        Assert.Equal(memberType, memberActual.PropertyType);
        Assert.Equal(memberUnique.Count, memberResult.Length);
    }

    [Theory(DisplayName = "Compare Inheritance Test")]
    [InlineData(typeof(IShadowingA), typeof(IShadowingC), 1)]
    [InlineData(typeof(IShadowingC), typeof(IShadowingB), -1)]
    [InlineData(typeof(IShadowingA), typeof(IShadowingB), 0)]
    [InlineData(typeof(IShadowingB), typeof(IShadowingA), 0)]
    [InlineData(typeof(CustomImplicitConversionTypeA), typeof(CustomImplicitConversionTypeB), 0)]
    public void CompareInheritanceTest(Type x, Type y, int expected)
    {
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "CompareInheritance");
        var reflectionFunction = (Comparison<Type>)Delegate.CreateDelegate(typeof(Comparison<Type>), reflectionMethod);

        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbolX = compilation.GetTypeByMetadataName(Assert.IsType<string>(x.FullName));
        var symbolY = compilation.GetTypeByMetadataName(Assert.IsType<string>(y.FullName));
        Assert.NotNull(symbolX);
        Assert.NotNull(symbolY);

        var symbolResult = Symbols.CompareInheritance(compilation, symbolX, symbolY);
        var memberResult = reflectionFunction.Invoke(x, y);
        Assert.Equal(expected, symbolResult);
        Assert.Equal(expected, memberResult);
    }

    public const string CompareInheritanceRequireReferenceTypeMessage = "Require reference type.";

    public const string CompareInheritanceIdenticalTypesDetectedMessage = "Identical types detected.";

    [Theory(DisplayName = "Compare Inheritance With Invalid Type Test")]
    [InlineData(typeof(float), typeof(long), CompareInheritanceRequireReferenceTypeMessage)]
    [InlineData(typeof(string), typeof(int), CompareInheritanceRequireReferenceTypeMessage)]
    [InlineData(typeof(double), typeof(Action), CompareInheritanceRequireReferenceTypeMessage)]
    [InlineData(typeof(string), typeof(string), CompareInheritanceIdenticalTypesDetectedMessage)]
    [InlineData(typeof(Action), typeof(Action), CompareInheritanceIdenticalTypesDetectedMessage)]
    [InlineData(typeof(IMessA0), typeof(IMessA0), CompareInheritanceIdenticalTypesDetectedMessage)]
    public void CompareInheritanceWithInvalidTypeTest(Type x, Type y, string message)
    {
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "CompareInheritance");
        var reflectionFunction = (Comparison<Type>)Delegate.CreateDelegate(typeof(Comparison<Type>), reflectionMethod);

        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbolX = compilation.GetTypeByMetadataName(Assert.IsType<string>(x.FullName));
        var symbolY = compilation.GetTypeByMetadataName(Assert.IsType<string>(y.FullName));
        Assert.NotNull(symbolX);
        Assert.NotNull(symbolY);

        var alpha = Assert.Throws<ArgumentException>(() => Symbols.CompareInheritance(compilation, symbolX, symbolY));
        var bravo = Assert.Throws<ArgumentException>(() => reflectionFunction.Invoke(x, y));
        Assert.Null(alpha.ParamName);
        Assert.Null(bravo.ParamName);
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }

    [Theory(DisplayName = "Get All Properties For Interface Type With Invalid Type Test")]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    public void GetAllPropertiesForInterfaceTypeWithInvalidTypeTest(Type type)
    {
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "GetAllPropertiesForInterfaceType");
        var reflectionFunction = (Func<Type, BindingFlags, ImmutableArray<MemberInfo>>)Delegate.CreateDelegate(typeof(Func<Type, BindingFlags, ImmutableArray<MemberInfo>>), reflectionMethod);

        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbol = compilation.GetTypeByMetadataName(Assert.IsType<string>(type.FullName));
        Assert.NotNull(symbol);

        var alpha = Assert.Throws<ArgumentException>(() => Symbols.GetAllPropertiesForInterfaceType(compilation, symbol, out _, CancellationToken.None));
        var bravo = Assert.Throws<ArgumentException>(() => reflectionFunction.Invoke(type, BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(alpha.ParamName);
        Assert.Null(bravo.ParamName);
        var message = "Require interface type.";
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }

    [Theory(DisplayName = "Get All Fields And Properties For Non Interface Type With Invalid Type Test")]
    [InlineData(typeof(IMessA0))]
    [InlineData(typeof(IShadowingA))]
    [InlineData(typeof(ICloneable))]
    public void GetAllFieldsAndPropertiesForNonInterfaceTypeWithInvalidTypeTest(Type type)
    {
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "GetAllFieldsAndPropertiesForNonInterfaceType");
        var reflectionFunction = (Func<Type, BindingFlags, ImmutableArray<MemberInfo>>)Delegate.CreateDelegate(typeof(Func<Type, BindingFlags, ImmutableArray<MemberInfo>>), reflectionMethod);

        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbol = compilation.GetTypeByMetadataName(Assert.IsType<string>(type.FullName));
        Assert.NotNull(symbol);

        var alpha = Assert.Throws<ArgumentException>(() => Symbols.GetAllFieldsAndPropertiesForNonInterfaceType(symbol, CancellationToken.None));
        var bravo = Assert.Throws<ArgumentException>(() => reflectionFunction.Invoke(type, BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(alpha.ParamName);
        Assert.Null(bravo.ParamName);
        var message = "Require not interface type.";
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }
}
