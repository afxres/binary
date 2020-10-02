module Values.ClassTypeTests

open Mikodev.Binary
open System
open System.Net
open Xunit

let generator = Generator.CreateDefault()

let testWithSpan (value : 'a) (expected : 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = new Allocator()
    converter.Encode(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)

    let span = ReadOnlySpan buffer
    let result = converter.Decode &span
    Assert.Equal<'a>(expected, result)
    ()

let testWithBytes (value : 'a) (expected : 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.Encode value
    Assert.Equal<byte>(bufferOrigin, buffer)

    let result = converter.Decode buffer
    Assert.Equal<'a>(expected, result)
    ()

let testAuto (value : 'a) (expected : 'a) =
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
    let length = PrimitiveHelper.DecodeNumber &anotherSpan
    Assert.Equal(bufferOrigin.Length, length)
    Assert.Equal<byte>(bufferOrigin, anotherSpan.ToArray())
    Assert.Equal(bufferOrigin.Length + 1, buffer.Length)
    ()

let testWithLengthPrefix (value : 'a) (expected : 'a) =
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
    let length = PrimitiveHelper.DecodeNumber &anotherSpan
    Assert.Equal(bufferOrigin.Length, length)
    Assert.Equal<byte>(bufferOrigin, anotherSpan.ToArray())
    Assert.Equal(bufferOrigin.Length + 1, buffer.Length)
    ()

let test (value : 'a) (expected : 'a) =
    let buffer = generator.Encode value
    let result : 'a = generator.Decode buffer
    Assert.Equal<'a>(expected, result)

    // convert via Converter
    testWithSpan value expected
    // converter via bytes methods
    testWithBytes value expected
    // convert via 'auto' methods
    testAuto value expected
    // convert with length prefix
    testWithLengthPrefix value expected
    ()

[<Theory>]
[<InlineData("sharp")>]
[<InlineData("上山打老虎")>]
let ``String Instance`` (text : string) =
    test text text
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData(null)>]
let ``String Empty Or Null`` (text : string) =
    test text String.Empty
    ()

[<Fact>]
let ``String From Default Value Of Span`` () =
    let converter = generator.GetConverter<string>()
    let span = new ReadOnlySpan<byte>()
    let result = converter.Decode(&span)
    Assert.Equal(String.Empty, result)
    ()

[<Theory>]
[<InlineData("ws://host:8080/chat/")>]
[<InlineData("Udp://SomeHost:2048")>]
[<InlineData("tcp://loop/q?=some")>]
[<InlineData("HTTP://bing.com")>]
let ``Uri Instance`` (data : string) =
    let item = new Uri(data)

    test item item

    let bufferAlpha = generator.Encode item
    let bufferBravo = generator.Encode data
    let result = generator.Decode<Uri> bufferAlpha
    Assert.Equal<byte>(bufferAlpha, bufferBravo)
    Assert.Equal(item, result)
    Assert.Equal(data, result.OriginalString)
    ()

[<Fact>]
let ``Uri Null`` () =
    let item : Uri = null

    test item item

    let buffer = generator.Encode item
    let result = generator.Decode<Uri> buffer

    let _ = Assert.Throws<UriFormatException> (fun () -> new Uri(String.Empty) |> ignore)
    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(item, result)
    ()

[<Fact>]
let ``Uri Null With Length Prefix`` () =
    let item : Uri = null
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
let ``IPAddress Instance`` (address : string) =
    let value = IPAddress.Parse address
    test value value
    ()

[<Fact>]
let ``IPAddress Null`` () =
    let address : IPAddress = null

    test address address

    let buffer = generator.Encode address
    let result : IPAddress = generator.Decode buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(address, result)
    ()
