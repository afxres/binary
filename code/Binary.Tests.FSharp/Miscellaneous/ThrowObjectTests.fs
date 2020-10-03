module Miscellaneous.ThrowObjectTests

open Mikodev.Binary
open System
open Xunit

let generator = Generator.CreateDefault()

type Test01 = Case01 of Tag : string

[<Fact>]
let ``Type With Duplicate Property Names`` () =
    let case = Case01 "01"
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter case |> ignore)
    let message = sprintf "Named object error, duplicate binary string keys detected, type: %O, string converter type: %O" typeof<Test01> (generator.GetConverter<string>().GetType())
    Assert.Equal(message, error.Message)
    ()

let testNoConstructor (source : 'a) =
    let converter = generator.GetConverter<'a>()
    let buffer = converter.Encode source
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode buffer |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" (source.GetType())
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Anonymous Class Record (case sensitive)`` () =
    testNoConstructor {| Alpha = "alpha"; alpha = 1 |}
    ()

[<Fact>]
let ``Anonymous Value Record (case sensitive)`` () =
    testNoConstructor struct {| bravo = 2; Bravo = "bravo" |}
    ()

type Class01 (key : int, Key : string) =
    member val key = key with get, set

    member val Key = Key with get, set

[<Fact>]
let ``Class Type With Same Name Properties (case sensitive, no suitable constructor)`` () =
    testNoConstructor (Class01(-1, "zero - 1"))
    ()

type Class02 (key : int, Key : string) =
    new () = Class02(0, String.Empty)

    member val key = key with get, set

    member val Key = Key with get, set

[<Fact>]
let ``Class Type With Same Name Properties (case sensitive)`` () =
    let source = Class02(3, "three")
    let converter = generator.GetConverter source
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal(3, result.key)
    Assert.Equal("three", result.Key)
    ()

[<Struct>]
type Value01 =
    val mutable private _key : int

    val mutable private _Key : string

    member me.key with get () = me._key and set x = me._key <- x

    member me.Key with get () = me._Key and set x = me._Key <- x

[<Fact>]
let ``Value Type With Same Name Properties (case sensitive)`` () =
    let source = Value01 (key = 4, Key = "four")
    let converter = generator.GetConverter source
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal(4, result.key)
    Assert.Equal("four", result.Key)
    ()
