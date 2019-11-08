module Classes.PrimitiveHelperTests

open Mikodev.Binary
open System
open System.Text
open Xunit

let random = Random()

let generator = Generator.CreateDefault()

[<Fact>]
let ``Encode Number From 0 To 63`` () =
    let buffer = Array.zeroCreate<byte> 1
    for i = 0 to 63 do
        let mutable allocator = Allocator(Span buffer)
        PrimitiveHelper.EncodeNumber(&allocator, i)
        let span = allocator.AsSpan()
        Assert.Equal(1, span.Length)
        Assert.Equal(i, int span.[0])
    ()

[<Fact>]
let ``Encode Number From 64 To 16383`` () =
    let buffer = Array.zeroCreate<byte> 2
    for i = 64 to 16383 do
        let mutable allocator = Allocator(Span buffer)
        PrimitiveHelper.EncodeNumber(&allocator, i)
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
let ``Encode Number From 16384`` (i : int) =
    let buffer = Array.zeroCreate<byte> 4
    let mutable allocator = Allocator(Span buffer)
    PrimitiveHelper.EncodeNumber(&allocator, i)
    let span = allocator.AsSpan()
    Assert.Equal(4, span.Length)
    Assert.Equal((i >>> 24) ||| 0x80, int span.[0])
    Assert.Equal((i >>> 16) |> byte, span.[1])
    Assert.Equal((i >>> 8) |> byte, span.[2])
    Assert.Equal((i >>> 0) |> byte, span.[3])
    ()

[<Theory>]
[<InlineData(0, 1)>]
[<InlineData(63, 1)>]
[<InlineData(64, 2)>]
[<InlineData(16383, 2)>]
[<InlineData(16384, 4)>]
[<InlineData(Int32.MaxValue, 4)>]
let ``Encode Number Then Decode`` (number : int, length : int) =
    let mutable allocator = Allocator()
    PrimitiveHelper.EncodeNumber(&allocator, number)
    let mutable span = allocator.AsSpan()
    Assert.Equal(length, span.Length)

    let result = PrimitiveHelper.DecodeNumber(&span)
    Assert.Equal(number, result)
    Assert.True(span.IsEmpty)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(Int32.MinValue)>]
let ``Encode Number (overflow)`` (number : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator()
        PrimitiveHelper.EncodeNumber(&allocator, number))
    Assert.Null(error.ParamName)
    Assert.Equal("Encode number can not be negative!", error.Message)
    ()

[<Fact>]
let ``Decode Number (empty bytes)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte>() in PrimitiveHelper.DecodeNumber(&span) |> ignore)
    Assert.Equal("Not enough bytes.", error.Message)
    ()

[<Fact>]
let ``Decode Number (not enough bytes)`` () =
    let test (bytes : byte array) =
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> let mutable span = ReadOnlySpan<byte> bytes in PrimitiveHelper.DecodeNumber(&span) |> ignore)
#if DEBUG
        Assert.Contains("ReadOnlySpan`1.Slice", error.StackTrace)
#endif
        ()

    test [| 64uy |]
    test [| 128uy; 0uy |]
    test [| 128uy; 0uy; 0uy |]
    ()

// string methods ↓↓↓

[<Theory>]
[<InlineData("The quick brown fox ...")>]
[<InlineData("今日はいい天気ですね")>]
let ``Encode String Then Decode`` (text : string) =
    let mutable allocator = new Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, span)
    let buffer = allocator.AsSpan().ToArray()
    let target = Converter.Encoding.GetBytes text
    Assert.Equal<byte>(buffer, target)

    let span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeString span
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("one two three four five")>]
[<InlineData("今晚打老虎")>]
let ``Encode String Then Decode (unicode)`` (text : string) =
    let mutable allocator = new Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, span, Encoding.Unicode)
    let buffer = allocator.AsSpan().ToArray()
    let target = Encoding.Unicode.GetBytes text
    Assert.Equal<byte>(buffer, target)

    let span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeString(span, Encoding.Unicode)
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("你好, 世界!")>]
let ``Encode String Then Decode (with length prefix)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span)
    let mutable span = allocator.AsSpan()
    let target = Converter.Encoding.GetBytes text
    let length = PrimitiveHelper.DecodeNumber(&span)
    Assert.Equal(target.Length, length)
    Assert.Equal(target.Length, span.Length)
    Assert.Equal<byte>(target, span.ToArray())

    let mutable span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeStringWithLengthPrefix(&span)
    Assert.True(span.IsEmpty)
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("Hello, world!")>]
[<InlineData("你好, 世界!")>]
let ``Encode String Then Decode (with length prefix, unicode)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span, Encoding.Unicode)
    let mutable span = allocator.AsSpan()
    let target = Encoding.Unicode.GetBytes text
    let length = PrimitiveHelper.DecodeNumber(&span)
    Assert.Equal(target.Length, length)
    Assert.Equal(target.Length, span.Length)
    Assert.Equal<byte>(target, span.ToArray())

    let mutable span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeStringWithLengthPrefix(&span, Encoding.Unicode)
    Assert.True(span.IsEmpty)
    Assert.Equal(text, result)
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
        PrimitiveHelper.EncodeString(&allocator, span)
        let buffer = allocator.AsSpan().ToArray()
        let result = encoding.GetString buffer
        Assert.Equal(text, result)
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
        PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span)
        let buffer = allocator.AsSpan().ToArray()
        let mutable span = ReadOnlySpan buffer
        let length = PrimitiveHelper.DecodeNumber(&span)
        let result = encoding.GetString (span.ToArray())
        let prefixLength = buffer.Length - length
        Assert.True(prefixLength > 0)
        Assert.Equal(i, span.Length)
        Assert.Equal(i, length)
        Assert.Equal(text, result)
    ()

[<Fact>]
let ``Encode String (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = new Allocator()
        let span = String.Empty.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, span, null)
        ())
    Assert.Equal("encoding", error.ParamName)
    ()

[<Fact>]
let ``Encode String (with length prefix, encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = new Allocator()
        let span = String.Empty.AsSpan()
        PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span, null)
        ())
    Assert.Equal("encoding", error.ParamName)
    ()

[<Fact>]
let ``Decode String (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let span = ReadOnlySpan Array.empty<byte>
        PrimitiveHelper.DecodeString(span, null) |> ignore
        ())
    Assert.Equal("encoding", error.ParamName)
    ()

[<Fact>]
let ``Decode String (with length prefix, encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable span = ReadOnlySpan Array.empty<byte>
        PrimitiveHelper.DecodeStringWithLengthPrefix(&span, null) |> ignore
        ())
    Assert.Equal("encoding", error.ParamName)
    ()
