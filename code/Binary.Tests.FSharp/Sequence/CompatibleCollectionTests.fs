module Sequence.CompatibleCollectionTests

open Mikodev.Binary
open System
open System.Collections.Concurrent
open System.Collections.Generic
open Xunit

let generator = Generator.CreateDefault()

let Test (value: 'a :> 'e seq) =
    let converter = generator.GetConverter<'a>()
    let buffer = converter.Encode value
    let result: 'a = converter.Decode buffer
    Assert.Equal<'e>(value, result :> 'e seq)

    let alpha = converter.Encode Unchecked.defaultof<'a>
    Assert.NotNull alpha
    Assert.Empty alpha

    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, Unchecked.defaultof<'a>)
    let bravo = allocator.AsSpan().ToArray()
    Assert.Equal(0uy, Assert.Single(bravo))
    ()

[<Fact>]
let ``Queue`` () =
    let alpha = [ 12..20 ] |> Queue
    let bravo = [ 32..40 ] |> List.map (sprintf "%d") |> Queue
    let delta = [ 'a' .. 'z' ] |> List.map (fun x -> struct (x, int x)) |> Queue
    let hotel = [ 'h' .. 'n' ] |> List.map (fun x -> (int64 x, x)) |> Queue

    Test alpha
    Test bravo
    Test delta
    Test hotel
    ()

[<Fact>]
let ``Stack (not supported)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<Stack<int>>() |> ignore)
    let message = sprintf "Invalid collection type: %O" typeof<Stack<int>>
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``ConcurrentStack (not supported)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<ConcurrentStack<string>>() |> ignore)
    let message = sprintf "Invalid collection type: %O" typeof<ConcurrentStack<string>>
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``ConcurrentDictionary`` () =
    let alpha = [ -4 .. 8 ] |> List.map (fun x -> KeyValuePair(x, sprintf "%x" x)) |> ConcurrentDictionary
    let bravo = [ "m16", 1.1; "m4", 2.0; "c4", 100.0 ] |> List.map (fun (x, y) -> KeyValuePair(x, y)) |> ConcurrentDictionary
    let delta = [ 3..21 ] |> List.map (fun x -> KeyValuePair(int64 x, double x)) |> ConcurrentDictionary

    Test alpha
    Test bravo
    Test delta
    ()

[<Fact>]
let ``SortedList`` () =
    let alpha = dict [ "v1", 1.0; "v2", 2.0; "vnext", Double.PositiveInfinity ] |> SortedList
    let bravo = dict [ DayOfWeek.Monday, "no"; DayOfWeek.Friday, "fine"; DayOfWeek.Sunday, "great" ] |> SortedList

    Test alpha
    Test bravo
    ()

[<Fact>]
let ``LinkedList`` () =
    let alpha = [ 16..32 ] |> LinkedList
    let bravo = [ 'a' .. 'z' ] |> List.map (fun x -> struct (x, int x)) |> LinkedList
    let delta = [ "alpha", "delta" ] |> LinkedList

    Test alpha
    Test bravo
    Test delta
    ()
