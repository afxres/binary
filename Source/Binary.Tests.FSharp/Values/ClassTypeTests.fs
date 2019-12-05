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

[<Theory>]
[<InlineData("127.0.0.1", 12345)>]
[<InlineData("::1", 54321)>]
let ``IPEndPoint Instance`` (address : string, port : int) =
    let value = new IPEndPoint(IPAddress.Parse(address), port)
    test value value
    ()

[<Fact>]
let ``IPEndPoint All Port`` () =
    for i = IPEndPoint.MinPort to IPEndPoint.MaxPort do
        let value = IPEndPoint(IPAddress.Any, i)
        test value value
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
let ``IPEndPoint Null`` () =
    let endpoint : IPEndPoint = null

    test endpoint endpoint

    let buffer = generator.Encode endpoint
    let result : IPEndPoint = generator.Decode buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(endpoint, result)
    ()

[<Fact>]
let ``IPEndPoint Not Enough Bytes`` () =
    let converter = generator.GetConverter<IPEndPoint>()
    let buffer = generator.Encode(struct (Unchecked.defaultof<IPAddress>, uint16 65535))
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode buffer |> ignore)
    let parameter = typeof<IPAddress>.GetConstructor([| typeof<byte[]> |]).GetParameters() |> Array.exactlyOne
    Assert.Equal(parameter.Name, error.ParamName)
    ()
