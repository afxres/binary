﻿module Creators.TupleTests

open Mikodev.Binary
open Mikodev.Binary.Abstractions
open System
open System.Net
open Xunit

let generator = new Generator()

let test (ls : int) (ll : int) (value : 'T) =
    let c = generator.GetConverter<'T> ()
    let mutable allocator = new Allocator()
    c.ToBytes(&allocator, value)
    let ba = allocator.ToArray()
    let ra = c.ToValue ba

    let mutable allocator = new Allocator()
    c.ToBytesWithMark(&allocator, value)
    let bb = allocator.ToArray()
    let mutable span = new ReadOnlySpan<byte>(bb)
    let rb = c.ToValueWithMark(&span)

    Assert.Equal<'T>(value, ra)
    Assert.Equal<'T>(value, rb)
    Assert.Equal(ls, Array.length ba)
    Assert.Equal(ll, Array.length bb)
    ()

let testNull<'T> () =
    let value = Unchecked.defaultof<'T>
    let converter = generator.GetConverter<'T> ()
    let message = sprintf "Tuple can not be null, type: %O" typeof<'T>
    let alpha = Assert.Throws<ArgumentNullException>(fun () -> let mutable allocator = new Allocator() in converter.ToBytes(&allocator, value))
    Assert.Equal("item", alpha.ParamName)
    Assert.StartsWith(message, alpha.Message)
    let bravo = Assert.Throws<ArgumentNullException>(fun () -> let mutable allocator = new Allocator() in converter.ToBytesWithMark(&allocator, value))
    Assert.Equal("item", bravo.ParamName)
    Assert.StartsWith(message, bravo.Message)
    ()

[<Fact>]
let ``Tuple Null 1`` () =
    testNull<Tuple<int>> ()
    ()

[<Fact>]
let ``Tuple Null 2`` () =
    testNull<string * string> ()
    ()

[<Fact>]
let ``Tuple Null 3`` () =
    testNull<int * double * string> ()
    ()

[<Fact>]
let ``Tuple Null 4`` () =
    testNull<int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 5`` () =
    testNull<int16 * int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 6`` () =
    testNull<byte * int16 * int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 7`` () =
    testNull<byte * int16 * int * double * Guid * string * IPAddress> ()
    ()

[<Fact>]
let ``Tuple Null 8`` () =
    testNull<byte * int16 * int * double * Guid * string * IPAddress * IPEndPoint> ()
    ()

[<Fact>]
let ``Value Tuple Empty Bytes`` () =
    Assert.Throws<ArgumentException>(fun () -> generator.ToValue<struct (int * int)> Array.empty |> ignore) |> ignore
    ()

[<Fact>]
let ``Tuple Array`` () =
    [ 1, "one"; 2, "two"; 3, "three" ] |> test 35 39
    [ struct (4, "four"); struct (5, "five") ] |> test 24 28
    ()

[<Fact>]
let ``Tuple 0`` () =
    let alpha = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<Tuple>() |> ignore)
    let bravo = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<ValueTuple>() |> ignore)
    Assert.Equal(sprintf "Invalid static type: %O" typeof<Tuple>, alpha.Message)
    Assert.Equal(sprintf "Invalid type: %O" typeof<ValueTuple>, bravo.Message)
    ()

[<Fact>]
let ``Tuple 1`` () =
    Tuple.Create<int> 1 |> test 4 4
    ValueTuple.Create 2.0 |> test 8 8
    Tuple.Create<string> "three" |> test 5 9
    ValueTuple.Create<string> "four" |> test 4 8
    ()
    
[<Fact>]
let ``Tuple 2`` () =
    (5, 6) |> test 8 8
    (struct (7, 8)) |> test 8 8
    (9, "zero") |> test 8 12
    (struct ("ten", 11)) |> test 11 11
    ()

[<Fact>]
let ``Tuple 3`` () =
    (1, 3, 5) |> test 12 12
    (struct (2.2, 4.8, 6.12)) |> test 24 24
    ("a", 22, "ccc") |> test 12 16
    (struct (4, "ee", 666)) |> test 14 14
    ()
    
[<Fact>]
let ``Tuple 4`` () =
    (9, 8, 7, 6) |> test 16 16
    (struct (5, 4, 3, 2)) |> test 16 16
    ("x", -1, "z", -2) |> test 18 18
    (struct (-2, "y", 3, "w")) |> test 14 18
    ()

[<Fact>]
let ``Tuple 5`` () =
    (2, 4, 6, 8, 0) |> test 20 20
    (struct (9, 7, 5, 3, 1)) |> test 20 20
    ("x", "y", "z", "w", -1) |> test 24 24
    (struct (16, "i", "j", "m", "n")) |> test 20 24
    ()

[<Fact>]
let ``Tuple 6`` () =
    (1, 2, 4, 8, 16, 32) |> test 24 24
    (struct (0, 1, 3, 7, 15, 31)) |> test 24 24
    ("t", "U", "p", "L", "e", 6) |> test 29 29
    (struct (8, "s", "h", "a", "r", "p")) |> test 25 29
    ()

[<Fact>]
let ``Tuple 7`` () =
    (9, 11, 13, 15, 17, 19, 21) |> test 28 28
    (struct (-2, 3, -5, 7, -11, 13, -17)) |> test 28 28
    ("alpha", 1024, "bravo", -65536, "charlie", 33, "delta") |> test 46 50
    (struct (7, "echo", "foxtrot", "golf", "hotel", 17, 19)) |> test 48 48
    ()

[<Fact>]
let ``Tuple 8`` () =
    (3, 4, 5, 6, 7, 8, 9, 0) |> test 32 32
    (struct (8, 7, 6, 5, 4, 3, 2, 1)) |> test 32 32
    ("v", 8, "a", 6, "l", 4, "u", "e") |> test 33 37
    (struct (-3, 6, -9, 12, -15, "no", "yes", "fine")) |> test 37 41
    ()

[<Fact>]
let ``Tuple N`` () =
    let a = (0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 9, 8)
    let b = struct ("ok", 25, "yes", "none", 2.0, "e", "pi", "data", 3.0F, 9y)
    let typeA = a.GetType()
    let typeB = b.GetType()
    Assert.Equal("Tuple`8", typeA.Name)
    Assert.Equal("ValueTuple`8", typeB.Name)
    Assert.True(typeA.GetProperties() |> Seq.exists (fun x -> x.Name = "Rest"))
    Assert.True(typeB.GetFields() |> Seq.exists (fun x -> x.Name = "Rest"))
    test 48 48 a
    test 57 57 b
    ()

let testSize<'T> size =
    let converter = generator.GetConverter<'T> ()
    Assert.Equal(size, converter.Length)
    ()

[<Fact>]
let ``Tuple Length`` () =
    testSize<int * int64> 12
    testSize<Tuple<string>> 0
    testSize<ValueTuple<string>> 0
    testSize<ValueTuple<int>> 4
    testSize<int * string> 0
    testSize<int * double * Guid> 28
    testSize<byte * int16 * int32 * uint64> 15
    testSize<int * int * int * int * int * int * int * int * int> 36
    ()


type FixType = { some : obj }

type FixConverter(length : int) =
    inherit ConstantConverter<FixType>(length)

    override __.ToBytes(_, _) = raise (NotSupportedException())

    override __.ToValue (_ : inref<ReadOnlySpan<byte>>) : FixType = raise (NotSupportedException())

[<Fact>]
let ``Tuple Length (overflow)`` () =
    let fixConverter = FixConverter(0x2000_0000) :> Converter
    let fixGenerator = Generator(converters = Array.singleton fixConverter)
    let alpha = fixGenerator.GetConverter<FixType * FixType>()
    let bravo = fixGenerator.GetConverter<struct (FixType * FixType * FixType)>()
    Assert.Throws<OverflowException>(fun () -> fixGenerator.GetConverter<FixType * FixType * FixType * FixType>() |> ignore) |> ignore
    Assert.Throws<OverflowException>(fun () -> fixGenerator.GetConverter<struct (FixType * FixType * FixType * FixType)>() |> ignore) |> ignore
    Assert.Equal(0x4000_0000, alpha.Length)
    Assert.Equal(0x6000_0000, bravo.Length)
    ()
