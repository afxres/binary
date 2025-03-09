namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class IConverterPlaceholderTests
{
    private delegate T DecodeDelegate<T>(ref ReadOnlySpan<byte> span);

    [Fact(DisplayName = "All Methods Test")]
    public void AllMethodsTest()
    {
        var placeholderType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "IConverterPlaceholder");
        var placeholderSharedInstanceInfo = placeholderType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
        Assert.NotNull(placeholderSharedInstanceInfo);
        var placeholder = Assert.IsType<IConverter>(placeholderSharedInstanceInfo.GetValue(null), exactMatch: false);

        static void InvokeEncode(AllocatorAction<object?> action)
        {
            var allocator = new Allocator();
            action.Invoke(ref allocator, new object());
            Assert.Fail();
        }

        static void InvokeDecode(DecodeDelegate<object?> decode)
        {
            var span = new ReadOnlySpan<byte>();
            _ = decode.Invoke(ref span);
            Assert.Fail();
        }

        _ = Assert.Throws<NotSupportedException>(() => placeholder.Length);
        _ = Assert.Throws<NotSupportedException>(() => placeholder.Encode(new object()));
        _ = Assert.Throws<NotSupportedException>(() => InvokeEncode(placeholder.Encode));
        _ = Assert.Throws<NotSupportedException>(() => InvokeEncode(placeholder.EncodeAuto));
        _ = Assert.Throws<NotSupportedException>(() => InvokeEncode(placeholder.EncodeWithLengthPrefix));
        _ = Assert.Throws<NotSupportedException>(() => placeholder.Decode(Array.Empty<byte>()));
        _ = Assert.Throws<NotSupportedException>(() => placeholder.Decode(new ReadOnlySpan<byte>()));
        _ = Assert.Throws<NotSupportedException>(() => InvokeDecode(placeholder.DecodeAuto));
        _ = Assert.Throws<NotSupportedException>(() => InvokeDecode(placeholder.DecodeWithLengthPrefix));
    }
}
