module Implementations.EnumerableTests

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open Xunit

let generator = new Generator()

type EmptyCollection<'T>() =
    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = raise (NotSupportedException())
        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException())

type EmptyDictionary<'K, 'V>() =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = raise (NotSupportedException())
        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K, 'V>> = raise (NotSupportedException())

[<Fact>]
let ``Bytes To Enumerable (no suitable constructor)`` () =
    let converter = generator.GetConverter<EmptyCollection<int>>()
    Assert.StartsWith("GenericCollectionConverter`2", converter.GetType().Name)
    let error = Assert.Throws<InvalidOperationException>(fun () -> converter.ToValue(Array.empty) |> ignore)
    Assert.Equal(sprintf "No suitable constructor found, type: %O" converter.ItemType, error.Message)
    ()

[<Fact>]
let ``Bytes To Enumerable (no suitable constructor, pair collection)`` () =
    let converter = generator.GetConverter<EmptyDictionary<int, string>>()
    Assert.StartsWith("GenericDictionaryConverter`3", converter.GetType().Name)
    let error = Assert.Throws<InvalidOperationException>(fun () -> converter.ToValue(Array.empty) |> ignore)
    Assert.Equal(sprintf "No suitable constructor found, type: %O" converter.ItemType, error.Message)
    ()
