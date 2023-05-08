namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.InheritanceTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
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

[SourceGeneratorContext]
[SourceGeneratorInclude<A>]
[SourceGeneratorInclude<B>]
[SourceGeneratorInclude<C>]
[SourceGeneratorInclude<D>]
[SourceGeneratorInclude<IO>]
[SourceGeneratorInclude<IP>]
[SourceGeneratorInclude<IQ>]
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

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(PlainObjectData))]
    [MemberData(nameof(PlainInterfaceObjectData))]
    public void EncodeDecodeTest<T, A>(Type wanted, T source, A anonymous)
    {
        var method = new Action<object, object>(EncodeDecodeTestInternal).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { wanted, typeof(A) });
        var result = target.Invoke(null, new object?[] { source, anonymous });
        Assert.Null(result);
    }
}
