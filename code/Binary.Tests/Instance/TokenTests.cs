namespace Mikodev.Binary.Tests.Instance;

using System;
using System.Collections.Generic;
using Xunit;

public class TokenTests
{
    private readonly IGenerator generator = Generator.CreateDefault();

    [Fact(DisplayName = "Invalid Memory Test")]
    public void InvalidMemoryTest()
    {
        // the leading byte announces a four byte length prefix, but no payload follows it
        var memory = new ReadOnlyMemory<byte>(new byte[] { 0x80 });
        var token = new Token(this.generator, memory);
        Assert.Empty(token.Children);
        Assert.Null(token.Parent);
        Assert.Equal(memory, token.Memory);

        // the failure is kept for the later member access instead of being thrown by the constructor
        var error = Assert.Throws<KeyNotFoundException>(() => _ = token["none"]);
        Assert.Equal("Key 'none' not found.", error.Message);
        var inner = Assert.IsType<ArgumentException>(error.InnerException);
        Assert.Equal("Not enough bytes or byte sequence invalid.", inner.Message);
    }
}
