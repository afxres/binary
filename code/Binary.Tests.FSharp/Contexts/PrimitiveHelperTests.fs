module Contexts.PrimitiveHelperTests

open Mikodev.Binary
open System
open System.Text
open Xunit

let random = Random()

let generator = Generator.CreateDefault()

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
    let length = Converter.Decode(&span)
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
        let length = Converter.Decode(&span)
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
        Converter.EncodeWithLengthPrefix(&allocator, ReadOnlySpan data)
        let buffer = allocator.AsSpan().ToArray()
        let mutable span = ReadOnlySpan buffer
        let text = PrimitiveHelper.DecodeStringWithLengthPrefix &span
        let result = encoding.GetBytes text
        Assert.Equal<byte>(data, result)
    ()
