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

type DictionaryR<'K, 'V>(item : Queue<KeyValuePair<'K, 'V>>) =
    interface IDictionary<'K, 'V> with
        member __.Add(key: 'K, value: 'V): unit = raise (System.NotImplementedException())

        member __.Add(item: KeyValuePair<'K,'V>): unit = raise (System.NotImplementedException())

        member __.Clear(): unit = raise (System.NotImplementedException())

        member __.Contains(item: KeyValuePair<'K,'V>): bool = raise (System.NotImplementedException())

        member __.ContainsKey(key: 'K): bool = raise (System.NotImplementedException())

        member __.CopyTo(array: KeyValuePair<'K,'V> [], arrayIndex: int): unit = item.CopyTo(array, arrayIndex)

        member __.Count: int = item.Count

        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

        member __.IsReadOnly: bool = raise (System.NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (System.NotImplementedException()) and set (key: 'K) (v: 'V): unit = raise (System.NotImplementedException())

        member __.Keys: ICollection<'K> = raise (System.NotImplementedException())

        member __.Remove(key: 'K): bool = raise (System.NotImplementedException())

        member __.Remove(item: KeyValuePair<'K,'V>): bool = raise (System.NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (System.NotImplementedException())

        member __.Values: ICollection<'V> = raise (System.NotImplementedException())

type DictionaryO<'K, 'V>(item : KeyValuePair<'K, 'V> array) =
    interface IReadOnlyDictionary<'K, 'V> with
        member __.ContainsKey(key: 'K): bool = raise (System.NotImplementedException())

        member __.Count: int = raise (System.NotImplementedException())

        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

        member __.Item with get (key: 'K): 'V = raise (System.NotImplementedException())

        member __.Keys: IEnumerable<'K> = raise (System.NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (System.NotImplementedException())

        member __.Values: IEnumerable<'V> = raise (System.NotImplementedException())

type DictionaryD<'K, 'V>(item : KeyValuePair<'K, 'V> ResizeArray) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

    interface IDictionary<'K, 'V> with
        member __.Add(key: 'K, value: 'V): unit = raise (System.NotImplementedException())

        member __.Add(item: KeyValuePair<'K,'V>): unit = raise (System.NotImplementedException())

        member __.Clear(): unit = raise (System.NotImplementedException())

        member __.Contains(item: KeyValuePair<'K,'V>): bool = raise (System.NotImplementedException())

        member __.ContainsKey(key: 'K): bool = raise (System.NotImplementedException())

        member __.CopyTo(array: KeyValuePair<'K,'V> [], arrayIndex: int): unit = item.CopyTo(array, arrayIndex)

        member __.Count: int = item.Count

        member __.IsReadOnly: bool = raise (System.NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (System.NotImplementedException()) and set (key: 'K) (v: 'V): unit = raise (System.NotImplementedException())

        member __.Keys: ICollection<'K> = raise (System.NotImplementedException())

        member __.Remove(key: 'K): bool = raise (System.NotImplementedException())

        member __.Remove(item: KeyValuePair<'K,'V>): bool = raise (System.NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (System.NotImplementedException())

        member __.Values: ICollection<'V> = raise (System.NotImplementedException())

    interface IReadOnlyDictionary<'K, 'V> with
        member __.ContainsKey(key: 'K): bool = raise (System.NotImplementedException())

        member __.Count: int = raise (System.NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (System.NotImplementedException())

        member __.Keys: IEnumerable<'K> = raise (System.NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (System.NotImplementedException())

        member __.Values: IEnumerable<'V> = raise (System.NotImplementedException())

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
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode(Array.empty) |> ignore)
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
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, constructor not match)`` () =
    test "EnumerableAdaptedConverter`2" "DelegateCollectionBuilder`2" (DictionaryP ((dict [ 1, "one"; 0, "ZERO" ]) |> Seq.toList)) [ 1, "one"; 0, "ZERO" ]
    ()

[<Fact>]
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, abstract)`` () =
    test "EnumerableAdaptedConverter`2" "DelegateCollectionBuilder`2" ((DictionaryI(dict [ 1, "one"; 0, "ZERO" ])) :> DictionaryA<_, _>) [ 1, "one"; 0, "ZERO" ]
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary', constructor not match)`` () =
    test "DictionaryAdaptedConverter`3" "DelegateDictionaryBuilder`3" ((DictionaryR(dict [ 1, "one"; 0, "ZERO" ] |> Queue<_>))) [ 1, "one"; 0, "ZERO" ]
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IReadOnlyDictionary', constructor not match)`` () =
    test "DictionaryAdaptedConverter`3" "DelegateDictionaryBuilder`3" ((DictionaryO(dict [ 1, "one"; 0, "ZERO" ] |> Seq.toArray))) [ 1, "one"; 0, "ZERO" ]
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary' and 'IReadOnlyDictionary', constructor not match)`` () =
    test "DictionaryAdaptedConverter`3" "DelegateDictionaryBuilder`3" ((DictionaryD(dict [ 1, "one"; 0, "ZERO" ] |> ResizeArray))) [ 1, "one"; 0, "ZERO" ]
    ()
