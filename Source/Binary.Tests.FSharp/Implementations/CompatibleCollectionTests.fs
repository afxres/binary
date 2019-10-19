module Implementations.CompatibleCollectionTests

open Mikodev.Binary
open System
open System.Collections.Concurrent
open System.Collections.Generic
open Xunit

let generator = new Generator()

let test (value : 'a when 'a :> 'e seq) =
    let buffer = generator.Encode value
    let result : 'a = generator.Decode buffer
    Assert.Equal<'e seq>(value, result)
    ()

[<Fact>]
let ``Queue`` () =
    let alpha = [ 12..20 ] |> Queue
    let bravo = [ 32..40 ] |> List.map (sprintf "%d") |> Queue
    let delta = [ 'a'..'z' ] |> List.map (fun x -> struct (x, int x)) |> Queue
    let hotel = [ 'h'..'n' ] |> List.map (fun x -> (int64 x, x)) |> Queue

    test alpha
    test bravo
    test delta
    test hotel
    ()

[<Fact>]
let ``Stack`` () =
    let alpha = [ 'a'..'z' ] |> Stack
    let bravo = [ 32..48 ] |> List.map (sprintf "%d") |> Stack

    test alpha
    test bravo
    ()

[<Fact>]
let ``ConcurrentStack`` () =
    let alpha = [ 'h'..'n' ] |> ConcurrentStack
    let bravo = [ 'o'..'z' ] |> Seq.map string |> ConcurrentStack

    test alpha
    test bravo
    ()

[<Fact>]
let ``ConcurrentDictionary`` () =
    let alpha = [ -4..8 ] |> List.map (fun x -> KeyValuePair(x, sprintf "%x" x)) |> ConcurrentDictionary
    let bravo = [ "m16", 1.1; "m4", 2.0; "c4", 100.0 ] |> List.map (fun (x, y) -> KeyValuePair(x, y)) |> ConcurrentDictionary

    test alpha
    test bravo
    ()

[<Fact>]
let ``SortedList`` () =
    let alpha = dict [ "v1", 1.0; "v2", 2.0; "vnext", Double.PositiveInfinity ] |> SortedList
    let bravo = dict [ DayOfWeek.Monday, "no"; DayOfWeek.Friday, "fine"; DayOfWeek.Sunday, "great" ] |> SortedList

    test alpha
    test bravo
    ()

[<Fact>]
let ``LinkedList`` () =
    let alpha = [ 16..32 ] |> LinkedList
    let bravo = [ 'a'..'z' ] |> List.map (fun x -> struct (x, int x)) |> LinkedList
    let delta = [ "alpha", "delta" ] |> LinkedList

    test alpha
    test bravo
    test delta
    ()
