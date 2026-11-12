#nowarn "864"

module Contexts.ConverterOverrideHideTests

open Mikodev.Binary
open System
open Xunit

/// F# option tags: 'None' is 0 (represented by null), 'Some' is 1
let private someTag = 1uy

type OverrideAllMember<'T>() =
    inherit Converter<'T>()

    override _.Decode(span: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("Fake 'Decode'"))

    override _.DecodeAuto(span: byref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("Fake 'DecodeAuto'"))

    override _.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("Fake 'DecodeWithLengthPrefix'"))

    override _.Encode(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("Fake 'Encode'"))

    override _.EncodeAuto(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("Fake 'EncodeAuto'"))

    override _.EncodeWithLengthPrefix(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("Fake 'EncodeWithLengthPrefix'"))

type OverrideHideAllMember<'T>() =
    inherit OverrideAllMember<'T>()

    member _.Decode(span: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("New 'Decode'"))

    member _.DecodeAuto(span: byref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("New 'DecodeAuto'"))

    member _.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("New 'DecodeWithLengthPrefix'"))

    member _.Encode(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("New 'Encode'"))

    member _.EncodeAuto(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("New 'EncodeAuto'"))

    member _.EncodeWithLengthPrefix(allocator: byref<Allocator>, item: 'T) : unit = raise (NotSupportedException("New 'EncodeWithLengthPrefix'"))

let private GetOptionConverter () : Converter<option<int>> =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().AddConverter(OverrideHideAllMember<int>()).Build()
    generator.GetConverter<option<int>>()

[<Fact>]
let ``Encode With New Encode Method`` () =
    let converter = GetOptionConverter()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)
    let error =
        Assert.Throws<NotSupportedException>(fun () ->
            let mutable allocator = Allocator()
            converter.Encode(&allocator, Some 0))
    Assert.Equal("Fake 'Encode'", error.Message)

[<Fact>]
let ``Encode With New Encode Auto Method`` () =
    let converter = GetOptionConverter()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)
    let error =
        Assert.Throws<NotSupportedException>(fun () ->
            let mutable allocator = Allocator()
            converter.EncodeAuto(&allocator, Some 0))
    Assert.Equal("Fake 'EncodeAuto'", error.Message)

[<Fact>]
let ``Decode With New Decode Method`` () =
    let converter = GetOptionConverter()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)
    let error =
        Assert.Throws<NotSupportedException>(fun () ->
            let span = ReadOnlySpan<byte>([| someTag |])
            converter.Decode(&span) |> ignore)
    Assert.Equal("Fake 'Decode'", error.Message)

[<Fact>]
let ``Decode With New Decode Auto Method`` () =
    let converter = GetOptionConverter()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)
    let error =
        Assert.Throws<NotSupportedException>(fun () ->
            let mutable span = ReadOnlySpan<byte>([| someTag |])
            converter.DecodeAuto(&span) |> ignore)
    Assert.Equal("Fake 'DecodeAuto'", error.Message)
