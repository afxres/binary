namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

public class NativeEndianTests
{
    private delegate bool IsNativeEndianConverterInternalDelegate(IConverter converter, bool isLittleEndian);

    private static IsNativeEndianConverterInternalDelegate GetIsNativeEndianConverterInternalDelegate()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "NativeEndian");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var result = Assert.IsType<IsNativeEndianConverterInternalDelegate>(Delegate.CreateDelegate(typeof(IsNativeEndianConverterInternalDelegate), method), exactMatch: false);
        return result;
    }

    [Fact(DisplayName = "Is Native Endian Converter Internal Integration Test")]
    public void IsNativeEndianConverterInternalIntegrationTest()
    {
        var generator = Generator.CreateDefault();
        var method = GetIsNativeEndianConverterInternalDelegate();
        Assert.False(method.Invoke(null!, false));
        Assert.False(method.Invoke(generator.GetConverter<string>(), true));
        Assert.False(method.Invoke(generator.GetConverter<List<int>>(), true));
        var a = generator.GetConverter<int>();
        var b = generator.GetConverter<Range>();
        Assert.Equal("LittleEndianConverter`1", a.GetType().Name);
        Assert.Equal("RepeatLittleEndianConverter`2", b.GetType().Name);
        Assert.True(method.Invoke(a, true));
        Assert.True(method.Invoke(b, true));
    }
}
