﻿module Sequence.EnumerableDecodeTests

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

type IFakeEnumerableInterface<'T> =
    inherit IEnumerable<'T>

type IFakeDictionaryInterface<'K, 'V> =
    inherit IDictionary<'K, 'V>

type FakeEnumerable<'T>(item : 'T list) =
    interface IFakeEnumerableInterface<'T>

    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<'T> = (item :> seq<_>).GetEnumerator()

[<AbstractClass>]
type FakeEnumerableAbstract<'T>(item : 'T seq) =
    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = item.GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<'T> = item.GetEnumerator()

type FakeEnumerableImplementation<'T>(item : 'T seq) =
    inherit FakeEnumerableAbstract<'T>(item)

type FakeEnumerableKeyValuePair<'K, 'V>(item : KeyValuePair<'K, 'V> list) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K, 'V>> = (item :> seq<_>).GetEnumerator()

[<AbstractClass>]
type FakeEnumerableKeyValuePairAbstract<'K, 'V>(item : IDictionary<'K, 'V>) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = item.GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K, 'V>> = item.GetEnumerator()

type FakeEnumerableKeyValuePairImplementation<'K, 'V>(item : IDictionary<'K, 'V>) =
    inherit FakeEnumerableKeyValuePairAbstract<'K, 'V>(item)

type FakeDictionary<'K, 'V>(item : Queue<KeyValuePair<'K, 'V>>) =
    interface IFakeDictionaryInterface<'K, 'V>

    interface IDictionary<'K, 'V> with
        member __.Add(key: 'K, value: 'V): unit = raise (NotImplementedException())

        member __.Add(item: KeyValuePair<'K,'V>): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: KeyValuePair<'K,'V>): bool = raise (NotImplementedException())

        member __.ContainsKey(key: 'K): bool = raise (NotImplementedException())

        member __.CopyTo(array: KeyValuePair<'K,'V> [], arrayIndex: int): unit = item.CopyTo(array, arrayIndex)

        member __.Count: int = item.Count

        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (NotImplementedException()) and set (key: 'K) (v: 'V): unit = raise (NotImplementedException())

        member __.Keys: ICollection<'K> = raise (NotImplementedException())

        member __.Remove(key: 'K): bool = raise (NotImplementedException())

        member __.Remove(item: KeyValuePair<'K,'V>): bool = raise (NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (NotImplementedException())

        member __.Values: ICollection<'V> = raise (NotImplementedException())

type FakeReadOnlyDictionary<'K, 'V>(item : KeyValuePair<'K, 'V> array) =
    interface IReadOnlyDictionary<'K, 'V> with
        member __.ContainsKey(key: 'K): bool = raise (NotImplementedException())

        member __.Count: int = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

        member __.Item with get (key: 'K): 'V = raise (NotImplementedException())

        member __.Keys: IEnumerable<'K> = raise (NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (NotImplementedException())

        member __.Values: IEnumerable<'V> = raise (NotImplementedException())

type FakeDictionaryReadOnlyDictionary<'K, 'V>(item : KeyValuePair<'K, 'V> ResizeArray) =
    interface IEnumerable<KeyValuePair<'K, 'V>> with
        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

    interface IDictionary<'K, 'V> with
        member __.Add(key: 'K, value: 'V): unit = raise (NotImplementedException())

        member __.Add(item: KeyValuePair<'K,'V>): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: KeyValuePair<'K,'V>): bool = raise (NotImplementedException())

        member __.ContainsKey(key: 'K): bool = raise (NotImplementedException())

        member __.CopyTo(array: KeyValuePair<'K,'V> [], arrayIndex: int): unit = item.CopyTo(array, arrayIndex)

        member __.Count: int = item.Count

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (NotImplementedException()) and set (key: 'K) (v: 'V): unit = raise (NotImplementedException())

        member __.Keys: ICollection<'K> = raise (NotImplementedException())

        member __.Remove(key: 'K): bool = raise (NotImplementedException())

        member __.Remove(item: KeyValuePair<'K,'V>): bool = raise (NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (NotImplementedException())

        member __.Values: ICollection<'V> = raise (NotImplementedException())

    interface IReadOnlyDictionary<'K, 'V> with
        member __.ContainsKey(key: 'K): bool = raise (NotImplementedException())

        member __.Count: int = raise (NotImplementedException())

        member __.Item with get (key: 'K): 'V = raise (NotImplementedException())

        member __.Keys: IEnumerable<'K> = raise (NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (NotImplementedException())

        member __.Values: IEnumerable<'V> = raise (NotImplementedException())

let Test (enumerable : 'a) (expected : 'b) (encoderName : string) =
    let converter = generator.GetConverter<'a>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`1", converter.GetType().Name)

    let encoder = converterType.GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let encoderActualName = encoder.Method.DeclaringType.Name
    Assert.Equal(encoderName, encoderActualName)
    let decoder = converterType.GetField("decode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let decoderMethod = decoder.Method
    // anonymous type
    Assert.Contains("<", decoderMethod.DeclaringType.Name)
    Assert.Contains(">", decoderMethod.DeclaringType.Name)
    Assert.Contains("<", decoderMethod.Name)
    Assert.Contains(">", decoderMethod.Name)

    let buffer = converter.Encode enumerable
    let target = generator.Encode expected
    Assert.Equal<byte>(buffer, target)
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode(Array.empty) |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<'a>
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``No suitable constructor (enumerable, custom interface)`` () =
    Test ((FakeEnumerable [ 1; 2; 3 ]) :> IFakeEnumerableInterface<_>) [ 1; 2; 3 ] "EnumerableEncoder`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable, constructor not match)`` () =
    Test ((FakeEnumerable [ 1; 2; 3 ])) [ 1; 2; 3 ] "EnumerableEncoder`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable, abstract)`` () =
    Test ((FakeEnumerableImplementation [ 1; 2; 3 ]) :> FakeEnumerableAbstract<_>) [ 1; 2; 3 ] "EnumerableEncoder`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, constructor not match)`` () =
    Test ((FakeEnumerableKeyValuePair ((dict [ 1, "one"; 0, "ZERO" ]) |> Seq.toList))) [ 1, "one"; 0, "ZERO" ] "EnumerableEncoder`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, abstract)`` () =
    Test ((FakeEnumerableKeyValuePairImplementation(dict [ 1, "one"; 0, "ZERO" ])) :> FakeEnumerableKeyValuePairAbstract<_, _>) [ 1, "one"; 0, "ZERO" ] "EnumerableEncoder`2"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary', custom interface)`` () =
    Test ((FakeDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Queue<_>)) :> IFakeDictionaryInterface<_, _>) [ 1, "one"; 0, "ZERO" ] "KeyValueEnumerableEncoder`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary', constructor not match)`` () =
    Test ((FakeDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Queue<_>))) [ 1, "one"; 0, "ZERO" ] "KeyValueEnumerableEncoder`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IReadOnlyDictionary', constructor not match)`` () =
    Test ((FakeReadOnlyDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Seq.toArray))) [ 1, "one"; 0, "ZERO" ] "KeyValueEnumerableEncoder`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary' and 'IReadOnlyDictionary', constructor not match)`` () =
    Test ((FakeDictionaryReadOnlyDictionary(dict [ 1, "one"; 0, "ZERO" ] |> ResizeArray))) [ 1, "one"; 0, "ZERO" ] "KeyValueEnumerableEncoder`3"
    ()
