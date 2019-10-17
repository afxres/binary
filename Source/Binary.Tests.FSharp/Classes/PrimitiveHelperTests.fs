module Classes.PrimitiveHelperTests

open Mikodev.Binary
open System
open System.Text
open Xunit

let random = Random()

let generator = Generator()

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
let ``Encode From 64 To 16383`` () =
    let buffer = Array.zeroCreate<byte> 2
    for i = 64 to 16383 do
        let mutable allocator = Allocator(buffer)
        PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
        let span = allocator.AsSpan()
        Assert.Equal(2, span.Length)
        Assert.Equal((i >>> 8) ||| 0x40, int span.[0])
        Assert.Equal(byte i, span.[1])
    ()

[<Theory>]
[<InlineData(0x0000_4000)>]
[<InlineData(0x0000_8642)>]
[<InlineData(0x00AB_CDEF)>]
[<InlineData(0x7FFF_FFFF)>]
let ``Encode From 16384`` (i : int) =
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
let ``Encode Then Decode From 0 To 65536`` () =
    let buffer = Array.zeroCreate<byte> 4
    for i = 0 to 65536 do
        let mutable allocator = Allocator(buffer)
        PrimitiveHelper.EncodeLengthPrefix(&allocator, uint32 i)
        let span = allocator.AsSpan()
        if i <= 0x3F then
            Assert.Equal(1, span.Length)
        elif i <= 0x3FFF then
            Assert.Equal(2, span.Length)
        else
            Assert.Equal(4, span.Length)
        let result = PrimitiveHelper.DecodeLengthPrefix(&span)
        Assert.Equal(i, result)
    ()

// string methods ↓↓↓

[<Theory>]
[<InlineData("The quick brown fox ...")>]
[<InlineData("今日はいい天気ですね")>]
let ``Encode String`` (text : string) =
    let mutable allocator = new Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, &span, Converter.Encoding)
    let buffer = allocator.ToArray()
    let result = Converter.Encoding.GetString buffer
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("one two three four five")>]
[<InlineData("今晚打老虎")>]
let ``Encode String (unicode)`` (text : string) =
    let mutable allocator = new Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, &span, Encoding.Unicode)
    let buffer = allocator.ToArray()
    let result = Encoding.Unicode.GetBytes text
    Assert.Equal<byte>(buffer, result)
    ()

[<Fact>]
let ``Encode String (random)`` () =
    let encoding = Converter.Encoding

    for i = 1 to 4096 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        let mutable allocator = new Allocator()
        let span = text.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, &span)
        let buffer = allocator.ToArray()
        let result = encoding.GetString buffer
        Assert.Equal(text, result)
#if DEBUG
        let capacity = if i <= 64 then encoding.GetMaxByteCount(i) else i
        Assert.Equal(capacity, allocator.Capacity)
#endif
    ()

[<Fact>]
let ``Encode String With Length Prefix (random)`` () =
    let encoding = Converter.Encoding

    for i = 1 to 4096 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        // MAKE ALLOCATOR MUTABLE!!!
        let mutable allocator = new Allocator()
        let span = text.AsSpan()
        PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, &span)
        let buffer = allocator.ToArray()
        let prefixLength = PrimitiveHelper.DecodePrefixLength(buffer.[0])
        let result = encoding.GetString (Array.skip prefixLength buffer)
        let span = ReadOnlySpan buffer
        Assert.Equal(text, result)
        Assert.Equal(i, PrimitiveHelper.DecodeLengthPrefix(&span))
#if DEBUG
        let capacity = if i <= 64 then encoding.GetMaxByteCount(i) else i
        Assert.Equal(capacity + prefixLength, allocator.Capacity)
#endif
    ()

[<Fact>]
let ``Encode String (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = new Allocator()
        let span = String.Empty.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, &span, null)
        ())
    Assert.Equal("encoding", error.ParamName)
    ()

[<Fact>]
let ``Encode String With Length Prefix (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = new Allocator()
        let span = String.Empty.AsSpan()
        PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, &span, null)
        ())
    Assert.Equal("encoding", error.ParamName)
    ()
