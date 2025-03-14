﻿module TupleLike.KeyValuePairTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let generator = Generator.CreateDefault()

let Encode<'a> (c: Converter<'a>) v =
    let mutable allocator = Allocator()
    c.Encode(&allocator, v)
    allocator.AsSpan().ToArray()

let EncodeAuto<'a> (c: Converter<'a>) v =
    let mutable allocator = Allocator()
    c.EncodeAuto(&allocator, v)
    allocator.AsSpan().ToArray()

let Decode<'a> (c: Converter<'a>) (buffer: byte array) =
    let span = ReadOnlySpan<byte>(buffer)
    c.Decode &span

let DecodeAuto<'a> (c: Converter<'a>) (buffer: byte array) =
    let mutable span = ReadOnlySpan<byte>(buffer)
    c.DecodeAuto &span

[<Theory>]
[<InlineData(16uy, "short", 0, 6, 7)>]
[<InlineData("", -2.4, 0, 9, 9)>]
[<InlineData(-4, 256L, 12, 12, 12)>]
[<InlineData("key", "value", 0, 9, 10)>]
let ``Key-Value Pair``<'K, 'V> (k: 'K) (v: 'V) define normal headed =
    let i = KeyValuePair(k, v)
    let t = k, v
    let alpha = generator.GetConverter<KeyValuePair<'K, 'V>>()
    let tuple = generator.GetConverter<'K * 'V>()

    let bka = Encode alpha i
    let rka = Decode alpha bka

    let bkb = EncodeAuto alpha i
    let rkb = DecodeAuto alpha bkb

    let bta = Encode tuple t
    let rta = Decode tuple bta

    let btb = EncodeAuto tuple t
    let rtb = DecodeAuto tuple btb

    Assert.Equal(define, alpha.Length)
    Assert.Equal(define, tuple.Length)

    Assert.Equal(normal, Array.length bka)
    Assert.Equal(headed, Array.length bkb)
    Assert.Equal(normal, Array.length bta)
    Assert.Equal(headed, Array.length btb)

    Assert.Equal(i, rka)
    Assert.Equal(i, rkb)
    Assert.Equal(t, rta)
    Assert.Equal(t, rtb)
    ()

[<Fact>]
let ``Key-Value Pair List`` () =
    let a = [ KeyValuePair<int, string>(1, "two"); KeyValuePair<int, string>(3, "four") ]
    let b = [ KeyValuePair<int16, int64>(int16 -1, int64 2); KeyValuePair<int16, int64>(int16 3, int64 -4) ]
    let bytesA = generator.Encode a
    let bytesB = generator.Encode b
    let seqA = generator.Decode<seq<int * string>> bytesA
    let seqB = generator.Decode<seq<int16 * int64>> bytesB
    Assert.Equal<int * string>(seqA, (a |> List.map (|KeyValue|)))
    Assert.Equal<int16 * int64>(seqB, (b |> List.map (|KeyValue|)))
    Assert.Equal(20, bytesB.Length)
    ()

[<Fact>]
let ``Key-Value Pair Array`` () =
    let alpha = [| KeyValuePair<byte, uint32>(byte 3, uint32 5); KeyValuePair<byte, uint32>(byte 7, uint32 9) |]
    let bytes = generator.Encode alpha
    let array = generator.Decode<array<byte * uint32>> bytes
    Assert.Equal<byte * uint32>(array, (alpha |> Array.map (|KeyValue|)))
    ()

type Raw<'a> = { data: 'a }

type RawConverter<'a>(length: int) =
    inherit Converter<Raw<'a>>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : Raw<'a> = raise (NotSupportedException())

[<Fact>]
let ``Key-Value Pair Length`` () =
    let singleConverter = RawConverter<single>(0x2000_0000) :> IConverter
    let doubleConverter = RawConverter<double>(0x4000_0000) :> IConverter
    let generator = Generator.CreateDefaultBuilder().AddConverter(singleConverter).AddConverter(doubleConverter).Build()
    let alpha = generator.GetConverter<KeyValuePair<Raw<single>, Raw<single>>>()
    let bravo = generator.GetConverter<KeyValuePair<Raw<single>, Raw<double>>>()
    Assert.Equal(0x4000_0000, alpha.Length)
    Assert.Equal(0x6000_0000, bravo.Length)
    ()

[<Fact>]
let ``Key-Value Pair Length (max value)`` () =
    let doubleConverter = RawConverter<double>(0x4000_0000) :> IConverter
    let stringConverter = RawConverter<string>(0x3FFF_FFFF) :> IConverter
    let generator = Generator.CreateDefaultBuilder().AddConverter(doubleConverter).AddConverter(stringConverter).Build()
    let delta = generator.GetConverter<KeyValuePair<Raw<double>, Raw<string>>>()
    Assert.Equal(Int32.MaxValue, delta.Length)
    ()

[<Fact>]
let ``Key-Value Pair Length (overflow)`` () =
    let doubleConverter = RawConverter<double>(0x4000_0000) :> IConverter
    let generator = Generator.CreateDefaultBuilder().AddConverter(doubleConverter).Build()
    Assert.Throws<OverflowException>(fun () -> generator.GetConverter<KeyValuePair<Raw<double>, Raw<double>>>() |> ignore) |> ignore
    ()
