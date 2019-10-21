module Implementations.EnumerableTests

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

type CollectionT<'T>(item : 'T list) =
    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<'T> = (item :> seq<_>).GetEnumerator()

[<AbstractClass>]
type CollectionA<'T>(item : 'T seq) =
    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = item.GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<'T> = item.GetEnumerator()

type CollectionI<'T>(item : 'T seq) =
    inherit CollectionA<'T>(item)

type DictionaryP<'K, 'V>(item : KeyValuePair<'K, 'V> list) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K, 'V>> = (item :> seq<_>).GetEnumerator()

[<AbstractClass>]
type DictionaryA<'K, 'V>(item : IDictionary<'K, 'V>) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = item.GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K, 'V>> = item.GetEnumerator()

type DictionaryI<'K, 'V>(item : IDictionary<'K, 'V>) =
    inherit DictionaryA<'K, 'V>(item)

let test (converterName : string) (builderName : string) (enumerable : 'a) (expected : 'b) =
    let converter = generator.GetConverter<'a>()
    Assert.StartsWith(converterName, converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().BaseType.GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let builder = builderField.GetValue(converter)
    Assert.Equal(builderName, builder.GetType().Name)

    let buffer = converter.Encode enumerable
    let target = generator.Encode expected
    Assert.Equal<byte>(buffer, target)
    let error = Assert.Throws<InvalidOperationException>(fun () -> converter.Decode(Array.empty) |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<'a>
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``No suitable constructor (enumerable, constructor not match)`` () =
    test "EnumerableAdaptedConverter`2" "DelegateCollectionBuilder`2" (CollectionT [ 1; 2; 3 ]) [ 1; 2; 3 ]
    ()

[<Fact>]
let ``No suitable constructor (enumerable, abstract)`` () =
    test "EnumerableAdaptedConverter`2" "DelegateCollectionBuilder`2" ((CollectionI [ 1; 2; 3 ]) :> CollectionA<_>) [ 1; 2; 3 ]
    ()

[<Fact>]
let ``No suitable constructor (dictionary, constructor not match)`` () =
    test "DictionaryAdaptedConverter`3" "DelegateDictionaryBuilder`3" (DictionaryP ((dict [ 1, "one"; 0, "ZERO" ]) |> Seq.toList)) [ 1, "one"; 0, "ZERO" ]
    ()

[<Fact>]
let ``No suitable constructor (dictionary, abstract)`` () =
    test "DictionaryAdaptedConverter`3" "DelegateDictionaryBuilder`3" ((DictionaryI(dict [ 1, "one"; 0, "ZERO" ])) :> DictionaryA<_, _>) [ 1, "one"; 0, "ZERO" ]
    ()
