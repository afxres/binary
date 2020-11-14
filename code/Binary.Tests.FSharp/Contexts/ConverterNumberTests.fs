module Contexts.ConverterNumberTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``Encode Number From 0 To 127`` () =
    let buffer = Array.zeroCreate<byte> 1
    for i = 0 to 127 do
        let mutable allocator = Allocator(Span buffer)
        Converter.Encode(&allocator, i)
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
    Converter.Encode(&allocator, i)
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
    Converter.Encode(&allocator, number)
    let mutable span = allocator.AsSpan()
    Assert.Equal(length, span.Length)

    let result = Converter.Decode(&span)
    Assert.Equal(number, result)
    Assert.True(span.IsEmpty)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(Int32.MinValue)>]
let ``Encode Number (overflow)`` (number : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Converter.Encode(&allocator, number))
    let methodInfo = typeof<Converter>.GetMethod("Encode")
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("number", error.ParamName)
    Assert.Equal("number", parameter.Name)
    Assert.StartsWith("Argument number must be greater than or equal to zero!", error.Message)
    ()

[<Fact>]
let ``Decode Number (empty bytes)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte>() in Converter.Decode(&span) |> ignore)
    let message = "Not enough bytes or byte sequence invalid."
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Decode Number (not enough bytes)`` () =
    let Test (bytes : byte array) =
        let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan<byte> bytes in Converter.Decode(&span) |> ignore)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.Equal(message, error.Message)
        ()

    Test Array.empty<byte>
    Test [| 128uy; 0uy |]
    Test [| 128uy; 0uy; 0uy |]
    ()
