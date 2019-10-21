module Values.ClassTypeTests

open Mikodev.Binary
open System
open System.Net
open Xunit

let generator = Generator.CreateDefault()

let testWithSpan (value : 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = new Allocator()
    converter.Encode(&allocator, value)
    let buffer = allocator.ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)

    let span = ReadOnlySpan buffer
    let result = converter.Decode &span
    Assert.Equal<'a>(value, result)
    ()

let testWithBytes (value : 'a) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.Encode value
    Assert.Equal<byte>(bufferOrigin, buffer)

    let result = converter.Decode buffer
    Assert.Equal<'a>(value, result)
    ()

let test (value : 'a) =
    let buffer = generator.Encode value
    let result : 'a = generator.Decode buffer
    Assert.Equal<'a>(value, result)

    // convert via Converter
    testWithSpan value
    // converter via bytes methods
    testWithBytes value
    ()

[<Theory>]
[<InlineData("sharp")>]
[<InlineData("上山打老虎")>]
let ``String Instance`` (text : string) =
    test text
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData(null)>]
let ``String (empty & null)`` (text : string) =
    let buffer = generator.Encode text
    let result : string = generator.Decode buffer

    Assert.Empty(buffer)
    Assert.Equal(String.Empty, result)
    ()

[<Fact>]
let ``String From Default Value Of Span`` () =
    let converter = generator.GetConverter<string>()
    let span = new ReadOnlySpan<byte>()
    let result = converter.Decode(&span)
    Assert.Equal(String.Empty, result)
    ()

[<Theory>]
[<InlineData("ws://localhost:8080/chat/")>]
[<InlineData("Udp://SomeHost:2048")>]
[<InlineData("tcp://loopback/query?=some")>]
[<InlineData("HTTP://bing.com")>]
let ``Uri Instance`` (data : string) =
    let item = new Uri(data)

    test item

    let bufferAlpha = generator.Encode item
    let bufferBravo = generator.Encode data
    let result = generator.Decode<Uri> bufferAlpha
    Assert.Equal<byte>(bufferAlpha, bufferBravo)
    Assert.Equal(item, result)
    Assert.Equal(data, result.OriginalString)

    ()

[<Fact>]
let ``Uri (null)`` () =
    let item : Uri = null
    let buffer = generator.Encode item
    let result = generator.Decode<Uri> buffer

    let _ = Assert.Throws<UriFormatException> (fun () -> new Uri(String.Empty) |> ignore)
    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(item, result)
    ()

[<Fact>]
let ``Uri (null, with length prefix)`` () =
    let item : Uri = null
    let converter = generator.GetConverter<Uri>()
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, item)
    let buffer = allocator.ToArray()
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
    test value
    ()

[<Fact>]
let ``IPAddress (null)`` () =
    let address : IPAddress = null
    let buffer = generator.Encode address
    let result : IPAddress = generator.Decode buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(address, result)
    ()

[<Theory>]
[<InlineData("127.0.0.1", 12345)>]
[<InlineData("::1", 54321)>]
let ``IPEndPoint Instance`` (address : string, port : int) =
    let value = new IPEndPoint(IPAddress.Parse(address), port)
    test value
    ()

[<Theory>]
[<InlineData("192.168.1.1", 15973)>]
[<InlineData("fe80::1", 62840)>]
let ``IPEndPoint And Tuple`` (address : string, port : int) =
    let address = IPAddress.Parse address
    let a = new IPEndPoint(address, port)
    let b = (address, uint16 port)
    let d = struct (address, uint16 port)
    let alpha = generator.Encode a
    let bravo = generator.Encode b
    let delta = generator.Encode d
    Assert.Equal<byte>(alpha, bravo)
    Assert.Equal<byte>(alpha, delta)
    ()

[<Fact>]
let ``IPEndPoint (null)`` () =
    let endpoint : IPEndPoint = null
    let buffer = generator.Encode endpoint
    let result : IPEndPoint = generator.Decode buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(endpoint, result)
    ()

[<Fact>]
let ``IPEndPoint (not enough bytes)`` () =
    let converter = generator.GetConverter<IPEndPoint>()
    let message = sprintf "Not enough bytes, type: %O" typeof<IPEndPoint>
    let buffer = generator.Encode(struct (Unchecked.defaultof<IPAddress>, uint16 65535))
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
    Assert.Equal(message, error.Message)
    ()
