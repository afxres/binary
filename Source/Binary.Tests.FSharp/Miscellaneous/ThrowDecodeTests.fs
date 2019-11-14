module Miscellaneous.ThrowDecodeTests

open Mikodev.Binary
open System
open Xunit

let message = "Not enough bytes or byte sequence invalid."

let outofrange = ArgumentOutOfRangeException().Message

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
    Assert.Equal(outofrange, error.Message)
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
    Assert.Equal(outofrange, error.Message)
    ()
