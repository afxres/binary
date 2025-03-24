namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

public class GeneratorAotTests
{
    [Fact(DisplayName = "To String (debug)")]
    public void CreateAot()
    {
        var generator = Generator.CreateAot();
        Assert.Matches(@"Converter Count = 1, Converter Creator Count = \d+", generator.ToString());
    }

    [Fact(DisplayName = "Get Converter (not supported)")]
    public void GetConverterNotSupported()
    {
        var generator = Generator.CreateAot();
        var error = Assert.Throws<NotSupportedException>(generator.GetConverter<ValueType>);
        Assert.Equal($"No available converter found, type: {typeof(ValueType)}", error.Message);
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        static Type[] GetArgs2(MethodInfo i)
        {
            if (i.Name.Contains("VariableBoundArray"))
                return [typeof(int[,]), typeof(int)];
            if (i.Name.Contains("Dictionary"))
                return [typeof(IEnumerable<int>), typeof(int)];
            else
                return [typeof(int), typeof(int)];
        }

        var methods = typeof(Generator).GetMethods().Where(x => x.Name.Contains("Converter")).ToList();
        var group1 = methods.Where(x => x.GetGenericArguments().Length is 1).ToList();
        var group2 = methods.Where(x => x.GetGenericArguments().Length is 2).ToList();

        var group1Methods = group1.Select(x => x.MakeGenericMethod(typeof(int))).ToList();
        var group2Methods = group2.Select(x => x.MakeGenericMethod(GetArgs2(x))).ToList();
        var result = group1Methods.Concat(group2Methods).ToList();
        Assert.NotEmpty(group1Methods);
        Assert.NotEmpty(group2Methods);
        Assert.Equal(methods.Count, result.Count);

        foreach (var i in result)
        {
            var parameters = i.GetParameters();
            if (parameters.Length is 0)
                continue;
            ArgumentTests.ArgumentNullExceptionTest(i);
        }
    }

    public static IEnumerable<object[]> EnumData()
    {
        yield return new object[] { DayOfWeek.Sunday };
        yield return new object[] { ConsoleKey.Clear };
        yield return new object[] { ConsoleColor.White };
    }

    [Theory(DisplayName = "Get Enum Converter Test")]
    [MemberData(nameof(EnumData))]
    public void GetEnumConverterTest<T>(T source) where T : unmanaged
    {
        var converter = Generator.GetEnumConverter<T>();
        Assert.Equal(Unsafe.SizeOf<T>(), converter.Length);
        var converterType = converter.GetType();
        Assert.Equal("LittleEndianConverter`1", converterType.Name);

        var buffer = converter.Encode(source);
        Assert.Equal(Unsafe.SizeOf<T>(), buffer.Length);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
    }

    public static IEnumerable<object[]> NonEnumData()
    {
        yield return new object[] { 0 };
        yield return new object[] { 1L };
    }

    [Theory(DisplayName = "Get Enum Converter Invalid Type")]
    [MemberData(nameof(NonEnumData))]
    public void GetEnumConverterInvalidType<T>(T source) where T : unmanaged
    {
        _ = source;
        var error = Assert.Throws<ArgumentException>(Generator.GetEnumConverter<T>);
        Assert.Null(error.ParamName);
        Assert.Equal("Require an enumeration type!", error.Message);
    }

    [Fact(DisplayName = "Get Variable Bound Array Converter Not Array Type")]
    public void GetVariableBoundArrayConverterNotArrayType()
    {
        var converter = Generator.GetEnumConverter<DayOfWeek>();
        var error = Assert.Throws<ArgumentException>(() => Generator.GetVariableBoundArrayConverter<string, DayOfWeek>(converter));
        Assert.Null(error.ParamName);
        Assert.Equal("Require variable bound array type.", error.Message);
    }

    [Fact(DisplayName = "Get Variable Bound Array Converter Not Variable Bound Array")]
    public void GetVariableBoundArrayConverterNotVariableBoundArray()
    {
        var converter = Generator.GetEnumConverter<DayOfWeek>();
        var error = Assert.Throws<ArgumentException>(() => Generator.GetVariableBoundArrayConverter<DayOfWeek[], DayOfWeek>(converter));
        Assert.Null(error.ParamName);
        Assert.Equal("Require variable bound array type.", error.Message);
    }

    [Fact(DisplayName = "Get Variable Bound Array Converter Element Type Not Match")]
    public void GetVariableBoundArrayConverterElementTypeNotMatch()
    {
        var converter = Generator.GetEnumConverter<DayOfWeek>();
        var error = Assert.Throws<ArgumentException>(() => Generator.GetVariableBoundArrayConverter<int[,], DayOfWeek>(converter));
        Assert.Null(error.ParamName);
        Assert.Equal("Element type not match.", error.Message);
    }
}
