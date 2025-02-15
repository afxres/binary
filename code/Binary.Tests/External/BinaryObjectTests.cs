namespace Mikodev.Binary.Tests.External;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

public class BinaryObjectTests
{
    private delegate object Create(ImmutableArray<ReadOnlyMemory<byte>> items, out int error);

    private static MethodInfo GetInternalMethod(string name)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
        Assert.NotNull(type);
        var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return method;
    }

    private static T GetInternalDelegate<T>(string name) where T : Delegate
    {
        var method = GetInternalMethod(name);
        Assert.NotNull(method);
        return (T)Delegate.CreateDelegate(typeof(T), method);
    }

    private static Create GetInternalCreateDelegate()
    {
        return GetInternalDelegate<Create>("Create");
    }

    [Fact(DisplayName = "Create Dictionary With Limited Item Count")]
    public void CreateItemCountLimited()
    {
        var create = GetInternalCreateDelegate();
        var a = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var b = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var da = create.Invoke([.. a.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString())))], out var ea);
        var db = create.Invoke([.. b.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString())))], out var eb);
        Assert.Equal("LongDataList", da.GetType().Name);
        Assert.Equal("HashCodeList", db.GetType().Name);
        Assert.Equal(-1, ea);
        Assert.Equal(-1, eb);
    }

    [Fact(DisplayName = "Create Dictionary With Limited Single Entry Length")]
    public void CreateItemSingleLengthLimited()
    {
        var create = GetInternalCreateDelegate();
        var a = new[] { "ShortName000015" };
        var b = new[] { "OtherName0000016" };
        var da = create.Invoke([.. a.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x)))], out var ea);
        var db = create.Invoke([.. b.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x)))], out var eb);
        Assert.Equal("LongDataList", da.GetType().Name);
        Assert.Equal("HashCodeList", db.GetType().Name);
        Assert.Equal(-1, ea);
        Assert.Equal(-1, eb);
    }
}
