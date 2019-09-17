module Primitives.ClassTypeTests

open Mikodev.Binary
open Mikodev.Binary.Abstractions
open System
open System.Net
open Xunit

let generator = new Generator()

let testWithSpan (value : 'a) =
    let bufferOrigin = generator.ToBytes value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = new Allocator()
    converter.ToBytes(&allocator, value)
    let buffer = allocator.ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)

    let span = ReadOnlySpan buffer
    let result = converter.ToValue &span
    Assert.Equal<'a>(value, result)
    ()

let testWithBytes (value : 'a) =
    let bufferOrigin = generator.ToBytes value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.ToBytes value
    Assert.Equal<byte>(bufferOrigin, buffer)

    let result = converter.ToValue buffer
    Assert.Equal<'a>(value, result)
    ()

let test (value : 'a) =
    let buffer = generator.ToBytes value
    let result : 'a = generator.ToValue buffer
    Assert.Equal<'a>(value, result)

    let converter = generator.GetConverter<'a>()
    Assert.IsAssignableFrom<VariableConverter<'a>> converter |> ignore

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
    let buffer = generator.ToBytes text
    let result : string = generator.ToValue buffer
    
    Assert.Empty(buffer)
    Assert.Equal(String.Empty, result)
    ()

[<Fact>]
let ``String From Default Value Of Span`` () =
    let converter = generator.GetConverter<string>()
    let span = new ReadOnlySpan<byte>()
    let result = converter.ToValue(&span)
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

    let bufferAlpha = generator.ToBytes item
    let bufferBravo = generator.ToBytes data
    let result = generator.ToValue<Uri> bufferAlpha
    Assert.Equal<byte>(bufferAlpha, bufferBravo)
    Assert.Equal(item, result)
    Assert.Equal(data, result.OriginalString)

    ()

[<Fact>]
let ``Uri (null)`` () =
    let item : Uri = null
    let buffer = generator.ToBytes item
    let result = generator.ToValue<Uri> buffer

    let _ = Assert.Throws<UriFormatException> (fun () -> new Uri(String.Empty) |> ignore)
    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(item, result)
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
    let buffer = generator.ToBytes address
    let result : IPAddress = generator.ToValue buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(address, result)
    ()

[<Fact>]
let ``IPAddress With Invalid Bytes`` () =
    let converter = generator.GetConverter<IPAddress>()
    let message = sprintf "Invalid bytes, type: %O" typeof<IPAddress>
    let errors = [
        for i in 1..128 do
            if i <> 4 && i <> 16 then
                let buffer = Array.zeroCreate<byte> i
                let error = Assert.Throws<ArgumentException>(fun () -> converter.ToValue buffer |> ignore)
                yield error.Message
    ]
    Assert.NotEmpty errors
    Assert.Equal(message, errors |> Set |> Assert.Single)
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
    let alpha = generator.ToBytes a
    let bravo = generator.ToBytes b
    let delta = generator.ToBytes d
    Assert.Equal<byte>(alpha, bravo |> Array.skip 4)
    Assert.Equal<byte>(alpha, delta |> Array.skip 4)
    ()

[<Fact>]
let ``IPEndPoint (null)`` () =
    let endpoint : IPEndPoint = null
    let buffer = generator.ToBytes endpoint
    let result : IPEndPoint = generator.ToValue buffer

    Assert.Null(result)
    Assert.Empty(buffer)
    Assert.Equal(endpoint, result)
    ()

[<Fact>]
let ``IPEndPoint With Invalid Bytes`` () =
    let converter = generator.GetConverter<IPEndPoint>()
    let message = sprintf "Invalid bytes, type: %O" typeof<IPEndPoint>
    let errors = [
        for i in 1..128 do
            if i <> 6 && i <> 18 then
                let buffer = Array.zeroCreate<byte> i
                let error = Assert.Throws<ArgumentException>(fun () -> converter.ToValue buffer |> ignore)
                yield error.Message
    ]
    Assert.NotEmpty errors
    Assert.Equal(message, errors |> Set |> Assert.Single)
    ()
