module Creators.KeyValuePairTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let generator = new Generator()

let bytes<'a> (c : Converter<'a>) v =
    let mutable allocator = new Allocator()
    c.ToBytes(&allocator, v)
    allocator.ToArray()

let bytesWithMark<'a> (c : Converter<'a>) v =
    let mutable allocator = new Allocator()
    c.ToBytesWithMark(&allocator, v)
    allocator.ToArray()

let value<'a> (c : Converter<'a>) buffer =
    let span = new ReadOnlySpan<byte>(buffer)
    c.ToValue &span

let valueWithMark<'a> (c : Converter<'a>) buffer =
    let mutable span = new ReadOnlySpan<byte>(buffer)
    c.ToValueWithMark &span

[<Theory>]
[<InlineData(16uy, "short", 0, 6, 10)>]
[<InlineData("", -2.4, 0, 12, 12)>]
[<InlineData(-4, 256L, 12, 12, 12)>]
[<InlineData("key", "value", 0, 12, 16)>]
let ``Key-Value Pair`` (k : 'K) (v : 'V) define normal headed =
    let i = KeyValuePair(k, v)
    let t = k, v
    let alpha = generator.GetConverter<KeyValuePair<'K, 'V>> ()
    let tuple = generator.GetConverter<'K * 'V> ()

    let bka = bytes alpha i
    let rka = value alpha bka

    let bkb = bytesWithMark alpha i
    let rkb = valueWithMark alpha bkb

    let bta = bytes tuple t
    let rta = value tuple bta

    let btb = bytesWithMark tuple t
    let rtb = valueWithMark tuple btb
   
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
    let a = [ new KeyValuePair<int, string>(1, "two"); new KeyValuePair<int, string>(3, "four") ]
    let b = [ new KeyValuePair<int16, int64>(int16 -1, int64 2); new KeyValuePair<int16, int64>(int16 3, int64 -4) ]
    let bytesA = generator.ToBytes a
    let bytesB = generator.ToBytes b
    let seqA = generator.ToValue<seq<int * string>> bytesA
    let seqB = generator.ToValue<seq<int16 * int64>> bytesB
    Assert.Equal<int * string>(seqA, (a |> List.map (|KeyValue|)))
    Assert.Equal<int16 * int64>(seqB, (b |> List.map (|KeyValue|)))
    Assert.Equal(20, bytesB.Length)
    ()

[<Fact>]
let ``Key-Value Pair Array`` () =
    let alpha = [| new KeyValuePair<byte, uint32>(byte 3, uint32 5); new KeyValuePair<byte, uint32>(byte 7, uint32 9) |]
    let bytes = generator.ToBytes alpha
    let array = generator.ToValue<array<byte * uint32>> bytes
    Assert.Equal<byte * uint32>(array, (alpha |> Array.map (|KeyValue|)))
    ()
