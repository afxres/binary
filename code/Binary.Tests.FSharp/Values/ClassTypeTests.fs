module Values.ClassTypeTests

open Mikodev.Binary
open System
open System.Net
open Xunit

let generator = Generator.CreateDefault()

let TestWithSpan (value: 'a) (expected: 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.Encode(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)

    let span = ReadOnlySpan buffer
    let result = converter.Decode &span
    Assert.Equal<'a>(expected, result)
    ()

let TestWithBytes (value: 'a) (expected: 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.Encode value
    Assert.Equal<byte>(bufferOrigin, buffer)

    let result = converter.Decode buffer
    Assert.Equal<'a>(expected, result)
    ()

let TestAuto (value: 'a) (expected: 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.EncodeAuto(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()

    let mutable span = ReadOnlySpan buffer
    let result = converter.DecodeAuto &span
    Assert.True(span.IsEmpty)
    Assert.Equal<'a>(expected, result)

    let mutable anotherSpan = ReadOnlySpan buffer
    let length = Converter.Decode &anotherSpan
    Assert.Equal(bufferOrigin.Length, length)
    Assert.Equal<byte>(bufferOrigin, anotherSpan.ToArray())
    Assert.Equal(bufferOrigin.Length + 1, buffer.Length)
    ()

let TestWithLengthPrefix (value: 'a) (expected: 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()

    let mutable span = ReadOnlySpan buffer
    let result = converter.DecodeWithLengthPrefix &span
    Assert.True(span.IsEmpty)
    Assert.Equal<'a>(expected, result)

    let mutable anotherSpan = ReadOnlySpan buffer
    let length = Converter.Decode &anotherSpan
    Assert.Equal(bufferOrigin.Length, length)
    Assert.Equal<byte>(bufferOrigin, anotherSpan.ToArray())
    Assert.Equal(bufferOrigin.Length + 1, buffer.Length)
    ()

let Test (value: 'a) (expected: 'a) =
    let buffer = generator.Encode value
    let result: 'a = generator.Decode buffer
    Assert.Equal<'a>(expected, result)

    // convert via Converter
    TestWithSpan value expected
    // converter via bytes methods
    TestWithBytes value expected
    // convert via 'auto' methods
    TestAuto value expected
    // convert with length prefix
    TestWithLengthPrefix value expected
    ()

[<Theory>]
[<InlineData("sharp")>]
[<InlineData("上山打老虎")>]
let ``String Instance`` (text: string) =
    Test text text
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData(null)>]
let ``String Empty Or Null`` (text: string) =
    Test text String.Empty
    ()

[<Fact>]
let ``String From Default Value Of Span`` () =
    let converter = generator.GetConverter<string>()
    let span = ReadOnlySpan<byte>()
    let result = converter.Decode(&span)
    Assert.Equal(String.Empty, result)
    ()

[<Theory>]
[<InlineData("ws://host:8080/chat/")>]
[<InlineData("Udp://SomeHost:2048")>]
[<InlineData("tcp://loop/q?=some")>]
[<InlineData("HTTP://bing.com")>]
let ``Uri Instance`` (data: string) =
    let item = Uri(data)

    Test item item

    let bufferAlpha = generator.Encode item
    let bufferBravo = generator.Encode data
    let result = generator.Decode<Uri> bufferAlpha
    Assert.Equal<byte>(bufferAlpha, bufferBravo)
    Assert.Equal(item, result)
    Assert.Equal(data, result.OriginalString)
    ()

[<Fact>]
let ``Uri Null`` () =
    let item: Uri = null

    Test item item

    let buffer = generator.Encode item
    let result = generator.Decode<Uri> buffer

    let _ = Assert.Throws<UriFormatException>(fun () -> Uri(String.Empty) |> ignore)
    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(item, result)
    ()

[<Fact>]
let ``Uri Null With Length Prefix`` () =
    let item: Uri = null
    let converter = generator.GetConverter<Uri>()
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, item)
    let buffer = allocator.AsSpan().ToArray()
    let mutable span = ReadOnlySpan buffer
    let result = converter.DecodeWithLengthPrefix(&span)

    Assert.Null(result)
    Assert.Equal(1, buffer.Length)
    Assert.True(span.IsEmpty)
    ()

[<Theory>]
[<InlineData("192.168.16.32")>]
[<InlineData("fe80::3c03:feef:ec25:e40d")>]
let ``IPAddress Instance`` (address: string) =
    let value = IPAddress.Parse address
    Test value value
    ()

[<Fact>]
let ``IPAddress Null`` () =
    let address: IPAddress = null

    Test address address

    let buffer = generator.Encode address
    let result: IPAddress = generator.Decode buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(address, result)
    ()
