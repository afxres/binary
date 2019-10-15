module Classes.PrimitiveHelperTests

open Mikodev.Binary
open Xunit

[<Fact>]
let ``Encode From 0 To 63`` () =
    let buffer = Array.zeroCreate<byte> 1
    for i = 0 to 63 do
        let mutable allocator = Allocator(buffer)
        PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
        let span = allocator.AsSpan()
        Assert.Equal(1, span.Length)
        Assert.Equal(i, int span.[0])
    ()

[<Fact>]
let ``Encode From 64 To 8191`` () =
    let buffer = Array.zeroCreate<byte> 2
    for i = 64 to 8191 do
        let mutable allocator = Allocator(buffer)
        PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
        let span = allocator.AsSpan()
        Assert.Equal(2, span.Length)
        Assert.Equal((i >>> 8) ||| 0x40, int span.[0])
        Assert.Equal(byte i, span.[1])
    ()

[<Theory>]
[<InlineData(0x0000_2000)>]
[<InlineData(0x0000_8642)>]
[<InlineData(0x00AB_CDEF)>]
[<InlineData(0x7FFF_FFFF)>]
let ``Encode More Than 8192`` (i : int) =
    let buffer = Array.zeroCreate<byte> 4
    let mutable allocator = Allocator(buffer)
    PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
    let span = allocator.AsSpan()
    Assert.Equal(4, span.Length)
    Assert.Equal((i >>> 24) ||| 0x80, int span.[0])
    Assert.Equal((i >>> 16) |> byte, span.[1])
    Assert.Equal((i >>> 8) |> byte, span.[2])
    Assert.Equal((i >>> 0) |> byte, span.[3])
    ()

[<Fact>]
let ``Encode Then Decode From 0 To 16384`` () =
    let buffer = Array.zeroCreate<byte> 4
    for i = 0 to 16384 do
        let mutable allocator = Allocator(buffer)
        PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
        let span = allocator.AsSpan()
        if i <= 0x3F then
            Assert.Equal(1, span.Length)
        elif i <= 0x1FFF then
            Assert.Equal(2, span.Length)
        else 
            Assert.Equal(4, span.Length)
        let result = PrimitiveHelper.DecodeLengthPrefix(&span)
        Assert.Equal(i, result)
    ()
