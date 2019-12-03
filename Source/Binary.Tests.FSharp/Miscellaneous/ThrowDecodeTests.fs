module Miscellaneous.ThrowDecodeTests

open Mikodev.Binary
open System
open Xunit

let message = "Not enough bytes or byte sequence invalid."

let outofrange = ArgumentOutOfRangeException().Message

let generator = Generator.CreateDefault()

[<Fact>]
let ``Decode Number (empty span)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan<byte>()
        PrimitiveHelper.DecodeNumber &span |> ignore)
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Number (invalid header)`` () =
    let buffer = [| 0x80uy |]
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        PrimitiveHelper.DecodeNumber &span |> ignore)
    Assert.StartsWith(outofrange, error.Message)
    ()

[<Fact>]
let ``Decode Buffer With Length Prefix (empty span)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan<byte>()
        let _ = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Buffer With Length Prefix (invalid header)`` () =
    let buffer = [| 0x40uy |]
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Buffer With Length Prefix (not enough bytes)`` () =
    let buffer = [| 0x01uy |]
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        ())
    Assert.StartsWith(outofrange, error.Message)
    ()

[<Fact>]
let ``Decode String With Length Prefix (empty span)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan<byte>()
        let _ = PrimitiveHelper.DecodeStringWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode String With Length Prefix (invalid header)`` () =
    let buffer = [| 0x40uy |]
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = PrimitiveHelper.DecodeStringWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode String With Length Prefix (not enough bytes)`` () =
    let buffer = [| 0x01uy |]
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = PrimitiveHelper.DecodeStringWithLengthPrefix &span
        ())
    Assert.StartsWith(outofrange, error.Message)
    ()

[<Fact>]
let ``Decode UInt32 With Length Prefix (empty span)`` () =
    let converter = generator.GetConverter<uint32>()
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan<byte>()
        let _ = converter.DecodeWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode UInt32 With Length Prefix (invalid header)`` () =
    let converter = generator.GetConverter<uint32>()
    let buffer = [| 0x40uy |]
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.DecodeWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode UInt32 With Length Prefix (not enough bytes)`` () =
    let converter = generator.GetConverter<uint32>()
    let buffer = [| 0x04uy |]
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.DecodeWithLengthPrefix &span
        ())
    Assert.StartsWith(outofrange, error.Message)
    ()

[<Fact>]
let ``Decode UInt32 With Length Prefix (length not match)`` () =
    let converter = generator.GetConverter<uint32>()
    let buffer = [| 0x01uy; 0uy; 0uy; 0uy; 0uy; |]
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.DecodeWithLengthPrefix &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Object (key, invalid header)`` () =
    let buffer = [| 0x40uy |]
    let converter = generator.GetConverter({| a = 0 |})
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.Decode &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Object (key, not enough bytes)`` () =
    let buffer = [| 0x01uy |]
    let converter = generator.GetConverter({| a = 0 |})
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.Decode &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Object (value, empty bytes)`` () =
    let buffer = [| 0x01uy; byte 'b'; |]
    let converter = generator.GetConverter({| a = 0 |})
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.Decode &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Object (value, invalid header)`` () =
    let buffer = [| 0x01uy; byte 'b'; 0x80uy |]
    let converter = generator.GetConverter({| a = 0 |})
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.Decode &span
        ())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Object (value, not enough bytes)`` () =
    let buffer = [| 0x01uy; byte 'b'; 0x02uy; 0x00uy |]
    let converter = generator.GetConverter({| a = 0 |})
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan buffer
        let _ = converter.Decode &span
        ())
    Assert.Equal(message, error.Message)
    ()
