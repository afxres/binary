module Contexts.ConverterMemoryTests

open Mikodev.Binary
open System
open Xunit

let random = Random()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(127)>]
[<InlineData(128)>]
[<InlineData(4096)>]
[<InlineData(65536)>]
let ``Encode Then Decode With Length Prefix`` (length : int) =
    let source = Array.zeroCreate<byte> length
    random.NextBytes source
    let mutable allocator = Allocator()
    Converter.EncodeWithLengthPrefix(&allocator, ReadOnlySpan source)
    let buffer = allocator.ToArray()
    let mutable span = ReadOnlySpan buffer
    let result = Converter.DecodeWithLengthPrefix &span
    Assert.Equal<byte>(source, result.ToArray())
    ()

[<Fact>]
let ``Decode With Length Prefix (empty bytes)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte>() in let _ = Converter.DecodeWithLengthPrefix(&span) in ())
    let message = "Not enough bytes or byte sequence invalid."
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode With Length Prefix (not enough bytes)`` () =
    let Test (bytes : byte array) =
        let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte>() in let _ = Converter.DecodeWithLengthPrefix(&span) in ())
        let message = "Not enough bytes or byte sequence invalid."
        Assert.Equal(message, error.Message)
        ()

    Test Array.empty<byte>
    Test [| 128uy; 0uy |]
    Test [| 128uy; 0uy; 0uy |]
    ()
