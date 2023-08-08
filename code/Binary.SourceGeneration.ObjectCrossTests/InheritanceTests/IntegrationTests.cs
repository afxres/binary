namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.InheritanceTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

abstract class A
{
    public abstract int H { get; set; }
}

class B : A
{
    public override int H { get; set; }

    public int I { get; set; }
}

class C : B
{
    public new string? I { get; set; }

    public virtual int J { get; set; }

    public virtual int K { get; set; }
}

class D : C
{
    public new string? K { get; set; }
}

interface IO
{
    int U { get; set; }
}

interface IP : IO
{
    int V { get; set; }
}

interface IQ : IP
{
    new string? V { get; set; }

    int W { get; set; }
}

class Q : IQ
{
    int IO.U { get; set; }

    int IP.V { get; set; }

    string? IQ.V { get; set; }

    int IQ.W { get; set; }
}

class IndexerA
{
    public int Item { get; set; }
}

class IndexerB : IndexerA
{
    public int this[int key] => throw new NotSupportedException();
}

class IndexerC : IndexerB
{
    public new int this[int key] => throw new NotSupportedException();

    public string this[string key] => throw new NotSupportedException();
}

class IndexerD : IndexerC
{
    public new string? Item { get; set; }

    [IndexerName("Location")]
    public long this[long key] => throw new NotSupportedException();
}

class IndexerE : IndexerD
{
    public long @this;

    public long Location;
}

class IndexerF : IndexerE
{
    [IndexerName("Position")]
    public new long this[long key] => throw new NotSupportedException();

    public new long Location { get; set; }
}

class SpecialMemberNameA
{
    public int yield;

    public int @this;

    public int @namespace;
}

class SpecialMemberNameB : SpecialMemberNameA
{
    public new string? @this { get; set; }

    public int @class { get; set; }

    public int join { get; set; }
}

class SpecialMemberNameC : SpecialMemberNameB
{
    public new string @class;

    public int async;

    public int await;

    public SpecialMemberNameC(string @class, int async, int await)
    {
        this.@class = @class;
        this.async = async;
        this.await = await;
    }
}

[SourceGeneratorContext]
[SourceGeneratorInclude<A>]
[SourceGeneratorInclude<B>]
[SourceGeneratorInclude<C>]
[SourceGeneratorInclude<D>]
[SourceGeneratorInclude<IO>]
[SourceGeneratorInclude<IP>]
[SourceGeneratorInclude<IQ>]
[SourceGeneratorInclude<IndexerA>]
[SourceGeneratorInclude<IndexerB>]
[SourceGeneratorInclude<IndexerC>]
[SourceGeneratorInclude<IndexerD>]
[SourceGeneratorInclude<IndexerE>]
[SourceGeneratorInclude<IndexerF>]
[SourceGeneratorInclude<SpecialMemberNameA>]
[SourceGeneratorInclude<SpecialMemberNameB>]
[SourceGeneratorInclude<SpecialMemberNameC>]
partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    private static void EncodeDecodeTestInternal<T, A>(T source, A anonymous)
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();
        var bufferExpected = generatorSecond.Encode(anonymous);

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(bufferExpected, buffer);
        Assert.Equal(bufferExpected, bufferSecond);

        if (typeof(T).IsAbstract)
        {
            var a = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
            var b = Assert.Throws<NotSupportedException>(() => converterSecond.Decode(bufferSecond));
            var message = $"No suitable constructor found, type: {typeof(T)}";
            Assert.Equal(message, a.Message);
            Assert.Equal(message, b.Message);
        }
        else
        {
            var result = converter.Decode(buffer);
            var resultSecond = converterSecond.Decode(buffer);
            Assert.Equal(bufferExpected, converter.Encode(result));
            Assert.Equal(bufferExpected, converterSecond.Encode(resultSecond));
        }
    }

    public static IEnumerable<object[]> PlainObjectData()
    {
        var b = new B { H = 1, I = 2 };
        yield return new object[] { typeof(A), b, new { H = 1 } };
        yield return new object[] { typeof(B), b, new { H = 1, I = 2 } };
        var d = new D { H = 3, I = "Four", J = 5, K = "Six" };
        ((B)d).I = 4;
        ((C)d).K = 6;
        yield return new object[] { typeof(B), d, new { H = 3, I = 4 } };
        yield return new object[] { typeof(C), d, new { H = 3, I = "Four", J = 5, K = 6 } };
        yield return new object[] { typeof(D), d, new { H = 3, I = "Four", J = 5, K = "Six" } };
    }

    public static IEnumerable<object[]> PlainInterfaceObjectData()
    {
        var q = new Q();
        ((IO)q).U = 1;
        ((IP)q).V = 2;
        ((IQ)q).V = "Two";
        ((IQ)q).W = 3;
        yield return new object[] { typeof(IO), q, new { U = 1 } };
        yield return new object[] { typeof(IP), q, new { V = 2 } };
        yield return new object[] { typeof(IQ), q, new { V = "Two", W = 3 } };
    }

    public static IEnumerable<object[]> PlainObjectWithIndexerData()
    {
        var d = new IndexerD { Item = "One" };
        ((IndexerA)d).Item = 1;
        yield return new object[] { typeof(IndexerA), d, new { Item = 1 } };
        yield return new object[] { typeof(IndexerB), d, new { Item = 1 } };
        yield return new object[] { typeof(IndexerC), d, new { Item = 1 } };
        yield return new object[] { typeof(IndexerD), d, new { Item = "One" } };
        var f = new IndexerF { Item = "Two", @this = 3, Location = 4 };
        ((IndexerA)f).Item = 5;
        ((IndexerE)f).Location = 6;
        yield return new object[] { typeof(IndexerA), f, new { Item = 5 } };
        yield return new object[] { typeof(IndexerE), f, new { Item = "Two", Location = 6L, @this = 3L } };
        yield return new object[] { typeof(IndexerF), f, new { Item = "Two", Location = 4L, @this = 3L } };
    }

    public static IEnumerable<object[]> PlainObjectWithSpecialMemberNameData()
    {
        var c = new SpecialMemberNameC("Zero", 1, 2);
        c.yield = 3;
        ((SpecialMemberNameA)c).@this = 4;
        c.@namespace = 5;
        c.@this = "Six";
        ((SpecialMemberNameB)c).@class = 7;
        c.join = 8;
        yield return new object[] { typeof(SpecialMemberNameA), c, new { @namespace = 5, @this = 4, yield = 3 } };
        yield return new object[] { typeof(SpecialMemberNameB), c, new { @class = 7, join = 8, @namespace = 5, @this = "Six", yield = 3 } };
        yield return new object[] { typeof(SpecialMemberNameC), c, new { async = 1, await = 2, @class = "Zero", join = 8, @namespace = 5, @this = "Six", yield = 3 } };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(PlainObjectData))]
    [MemberData(nameof(PlainInterfaceObjectData))]
    [MemberData(nameof(PlainObjectWithIndexerData))]
    [MemberData(nameof(PlainObjectWithSpecialMemberNameData))]
    public void EncodeDecodeTest<T, A>(Type wanted, T source, A anonymous)
    {
        var method = new Action<object, object>(EncodeDecodeTestInternal).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { wanted, typeof(A) });
        var result = target.Invoke(null, new object?[] { source, anonymous });
        Assert.Null(result);
    }

    [Theory(DisplayName = "Get All Fields And Properties Test")]
    [MemberData(nameof(PlainObjectData))]
    [MemberData(nameof(PlainInterfaceObjectData))]
    [MemberData(nameof(PlainObjectWithIndexerData))]
    [MemberData(nameof(PlainObjectWithSpecialMemberNameData))]
    public void GetAllFieldsAndPropertiesTest<T, A>(Type wanted, T source, A anonymous)
    {
        Assert.NotNull(wanted);
        Assert.NotNull(source);
        Assert.NotNull(anonymous);
        var reflectionModule = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "CommonModule");
        var reflectionMethod = reflectionModule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "GetAllFieldsAndProperties");
        var reflectionFunction = (Func<Type, BindingFlags, ImmutableArray<MemberInfo>>)Delegate.CreateDelegate(typeof(Func<Type, BindingFlags, ImmutableArray<MemberInfo>>), reflectionMethod);

        var fullName = wanted.FullName;
        Assert.NotNull(fullName);
        var compilation = CompilationModule.CreateCompilationFromThisAssembly();
        var symbol = compilation.GetTypeByMetadataName(fullName);
        Assert.NotNull(symbol);

        var anonymousMembers = typeof(A).GetMembers();
        var anonymousFieldsAndProperties = anonymousMembers.Where(x => x is FieldInfo or PropertyInfo).ToList();
        var anonymousFieldsAndPropertiesNames = anonymousFieldsAndProperties.Select(x => x.Name).ToHashSet();
        Assert.NotEmpty(anonymousFieldsAndPropertiesNames);

        var symbolFieldsAndProperties = Symbols.GetAllFieldsAndProperties(symbol, default);
        var wantedFieldsAndProperties = reflectionFunction.Invoke(wanted, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotEmpty(symbolFieldsAndProperties);
        Assert.NotEmpty(wantedFieldsAndProperties);
        Assert.Equal(symbolFieldsAndProperties.Length, wantedFieldsAndProperties.Length);
        Assert.All(wantedFieldsAndProperties, x => Assert.Equal(x.DeclaringType, x.ReflectedType));

        var symbolMatches = symbolFieldsAndProperties.Where(x => x is not IPropertySymbol property || property.IsIndexer is false);
        var wantedMatches = wantedFieldsAndProperties.Where(x => x is not PropertyInfo property || property.GetIndexParameters().Length is 0);
        Assert.Equal(anonymousFieldsAndPropertiesNames, symbolMatches.Select(x => x.Name).ToHashSet());
        Assert.Equal(anonymousFieldsAndPropertiesNames, wantedMatches.Select(x => x.Name).ToHashSet());
    }
}
