module Contexts.PrimitiveHelperTests

open Mikodev.Binary
open System
open System.Text
open Xunit

let random = Random()

let generator = Generator.CreateDefault()

[<Fact>]
let ``Encode Number From 0 To 127`` () =
    let buffer = Array.zeroCreate<byte> 1
    for i = 0 to 127 do
        let mutable allocator = Allocator(Span buffer)
        PrimitiveHelper.EncodeNumber(&allocator, i)
        let span = allocator.AsSpan()
        Assert.Equal(1, span.Length)
        Assert.Equal(i, int span.[0])
    ()

[<Theory>]
[<InlineData(0x0000_0080)>]
[<InlineData(0x0000_8642)>]
[<InlineData(0x00AB_CDEF)>]
[<InlineData(0x7FFF_FFFF)>]
let ``Encode Number From 128`` (i : int) =
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
[<InlineData(127, 1)>]
[<InlineData(128, 4)>]
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
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        PrimitiveHelper.EncodeNumber(&allocator, number))
    let methodInfo = typeof<PrimitiveHelper>.GetMethod("EncodeNumber")
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("number", error.ParamName)
    Assert.Equal("number", parameter.Name)
    Assert.StartsWith("Argument number must be greater than or equal to zero!", error.Message)
    ()

[<Fact>]
let ``Decode Number (empty bytes)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte>() in PrimitiveHelper.DecodeNumber(&span) |> ignore)
    let message = "Not enough bytes or byte sequence invalid."
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Number (not enough bytes)`` () =
    let Test (bytes : byte array) =
        let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte> bytes in PrimitiveHelper.DecodeNumber(&span) |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.Equal(message, error.Message)
        ()

    Test Array.empty<byte>
    Test [| 128uy; 0uy |]
    Test [| 128uy; 0uy; 0uy |]
    ()

// string methods ↓↓↓

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, 世界")>]
[<InlineData("今日はいい天気ですね")>]
let ``Encode String Then Decode`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, span)
    let buffer = allocator.AsSpan().ToArray()
    let target = Encoding.UTF8.GetBytes text
    Assert.Equal<byte>(buffer, target)

    let span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeString span
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, world!")>]
[<InlineData("你好, 世界!")>]
let ``Encode String Then Decode With Length Prefix`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span)
    let mutable span = allocator.AsSpan()
    let target = Encoding.UTF8.GetBytes text
    let length = PrimitiveHelper.DecodeNumber(&span)
    Assert.Equal(target.Length, length)
    Assert.Equal(target.Length, span.Length)
    Assert.Equal<byte>(target, span.ToArray())

    let mutable span = allocator.AsSpan()
    let result = PrimitiveHelper.DecodeStringWithLengthPrefix(&span)
    Assert.True(span.IsEmpty)
    Assert.Equal(text, result)
    ()

[<Fact>]
let ``Encode String (null)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    PrimitiveHelper.EncodeString(&allocator, span);
    Assert.Equal(0, allocator.Length)
    ()

[<Fact>]
let ``Encode String With Length Prefix (null)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    PrimitiveHelper.EncodeStringWithLengthPrefix(&allocator, span);
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal(byte 0, Assert.Single(buffer))
    ()

[<Fact>]
let ``Encode String (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        let mutable allocator = Allocator()
        let span = text.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, span)
        let buffer = allocator.AsSpan().ToArray()
        let result = encoding.GetString buffer
        Assert.Equal(text, result)
    ()

[<Fact>]
let ``Decode String (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield byte (random.Next(32, 127)) |]
        Assert.Equal(i, data.Length)

        let text = PrimitiveHelper.DecodeString(ReadOnlySpan data)
        let result = encoding.GetBytes text
        Assert.Equal<byte>(data, result)
    ()

[<Fact>]
let ``Encode String With Length Prefix (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        let mutable allocator = Allocator()
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
let ``Decode String With Length Prefix (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield byte (random.Next(32, 127)) |]
        Assert.Equal(i, data.Length)

        let mutable allocator = Allocator()
        PrimitiveHelper.EncodeBufferWithLengthPrefix(&allocator, ReadOnlySpan data)
        let buffer = allocator.AsSpan().ToArray()
        let mutable span = ReadOnlySpan buffer
        let text = PrimitiveHelper.DecodeStringWithLengthPrefix &span
        let result = encoding.GetBytes text
        Assert.Equal<byte>(data, result)
    ()
