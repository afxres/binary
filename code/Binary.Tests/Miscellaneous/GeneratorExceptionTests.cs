namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using Xunit;

public class GeneratorExceptionTests
{
#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value
    private unsafe class ClassWithPointerMember
    {
        public int* Pointer;
    }

    private unsafe class ClassWithFunctionPointerMember
    {
        public delegate*<long, long> FunctionPointer;
    }
#pragma warning restore CS0649 // Field '...' is never assigned to, and will always have its default value

    [Fact(DisplayName = "Invalid Pointer Type")]
    public void InvalidPointerType()
    {
        var generator = Generator.CreateDefault();
        var a = Assert.Throws<ArgumentException>(() => generator.GetConverter(typeof(int*)));
        var b = Assert.Throws<ArgumentException>(generator.GetConverter<ClassWithPointerMember>);
        Assert.Null(a.ParamName);
        Assert.Null(b.ParamName);
        var message = $"Invalid pointer type: {typeof(int*)}";
        Assert.Equal(message, a.Message);
        Assert.Equal(message, b.Message);
    }

    [Fact(DisplayName = "Invalid Function Pointer Type")]
    public void InvalidFunctionPointerType()
    {
        var generator = Generator.CreateDefault();
        var a = Assert.Throws<ArgumentException>(() => generator.GetConverter(typeof(delegate*<long, long>)));
        var b = Assert.Throws<ArgumentException>(generator.GetConverter<ClassWithFunctionPointerMember>);
        Assert.Null(a.ParamName);
        Assert.Null(b.ParamName);
        var message = $"Invalid function pointer type: {typeof(delegate*<long, long>)}";
        Assert.Equal(message, a.Message);
        Assert.Equal(message, b.Message);
    }
}
