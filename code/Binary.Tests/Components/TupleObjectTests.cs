namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Reflection;
using Xunit;

public class TupleObjectTests
{
    private sealed class FakeConverter<T> : Converter<T>
    {
        public FakeConverter(int length) : base(length) { }

        public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotImplementedException();
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(TupleObject), "GetTupleObjectLength", BindingFlags.Static | BindingFlags.Public);
        var error = Assert.Throws<ArgumentNullException>(() => TupleObject.GetTupleObjectLength(null!));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
    }

    [Fact(DisplayName = "Overflow Test")]
    public void OverflowTest()
    {
        var converter = new FakeConverter<int>(0x4000_0000);
        var error = Assert.Throws<OverflowException>(() => TupleObject.GetTupleObjectLength(new[] { converter, converter }));
        Assert.Equal(new OverflowException().Message, error.Message);
    }
}
