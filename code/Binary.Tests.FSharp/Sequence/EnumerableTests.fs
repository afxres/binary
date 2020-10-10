module Sequence.EnumerableTests

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

type FakeReadOnlyDictionary<'K, 'V>(item : KeyValuePair<'K, 'V> array) =
    interface IReadOnlyDictionary<'K, 'V> with
        member __.ContainsKey(key: 'K): bool = raise (System.NotImplementedException())

        member __.Count: int = raise (System.NotImplementedException())

        member __.GetEnumerator(): IEnumerator = (item :> seq<_>).GetEnumerator() :> IEnumerator

        member __.GetEnumerator(): IEnumerator<KeyValuePair<'K,'V>> = (item :> seq<_>).GetEnumerator()

        member __.Item with get (key: 'K): 'V = raise (System.NotImplementedException())

        member __.Keys: IEnumerable<'K> = raise (System.NotImplementedException())

        member __.TryGetValue(key: 'K, value: byref<'V>): bool = raise (System.NotImplementedException())

        member __.Values: IEnumerable<'V> = raise (System.NotImplementedException())

type FakeDictionaryReadOnlyDictionary<'K, 'V>(item : KeyValuePair<'K, 'V> ResizeArray) =
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

let Test (enumerable : 'a) (expected : 'b) (adaptedType : Type) (adapterName : string) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("SequenceConverter`2", converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let builder = builderField.GetValue converter
    Assert.Equal("DelegateBuilder`2", builder.GetType().Name)
    let builderGenericArguments = builder.GetType().GetGenericArguments()
    Assert.Equal(adaptedType, builderGenericArguments |> Array.last)

    let adapterField = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let adapter = adapterField.GetValue converter
    Assert.Equal(adapterName, adapter.GetType().Name)

    let buffer = converter.Encode enumerable
    let target = generator.Encode expected
    Assert.Equal<byte>(buffer, target)
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode(Array.empty) |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<'a>
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``No suitable constructor (enumerable, custom interface)`` () =
    Test ((FakeEnumerable [ 1; 2; 3 ]) :> IFakeEnumerableInterface<_>) [ 1; 2; 3 ] typeof<ArraySegment<int>> "EnumerableAdapter`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable, constructor not match)`` () =
    Test ((FakeEnumerable [ 1; 2; 3 ])) [ 1; 2; 3 ] typeof<ArraySegment<int>> "EnumerableAdapter`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable, abstract)`` () =
    Test ((FakeEnumerableImplementation [ 1; 2; 3 ]) :> FakeEnumerableAbstract<_>) [ 1; 2; 3 ] typeof<ArraySegment<int>> "EnumerableAdapter`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, constructor not match)`` () =
    Test ((FakeEnumerableKeyValuePair ((dict [ 1, "one"; 0, "ZERO" ]) |> Seq.toList))) [ 1, "one"; 0, "ZERO" ] typeof<ArraySegment<KeyValuePair<int, string>>> "EnumerableAdapter`2"
    ()

[<Fact>]
let ``No suitable constructor (enumerable with 'KeyValuePair' sequence constructor, abstract)`` () =
    Test ((FakeEnumerableKeyValuePairImplementation(dict [ 1, "one"; 0, "ZERO" ])) :> FakeEnumerableKeyValuePairAbstract<_, _>) [ 1, "one"; 0, "ZERO" ] typeof<ArraySegment<KeyValuePair<int, string>>> "EnumerableAdapter`2"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary', custom interface)`` () =
    Test ((FakeDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Queue<_>)) :> IFakeDictionaryInterface<_, _>) [ 1, "one"; 0, "ZERO" ] typeof<IEnumerable<KeyValuePair<int, string>>> "KeyValueEnumerableAdapter`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary', constructor not match)`` () =
    Test ((FakeDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Queue<_>))) [ 1, "one"; 0, "ZERO" ] typeof<IEnumerable<KeyValuePair<int, string>>> "KeyValueEnumerableAdapter`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IReadOnlyDictionary', constructor not match)`` () =
    Test ((FakeReadOnlyDictionary(dict [ 1, "one"; 0, "ZERO" ] |> Seq.toArray))) [ 1, "one"; 0, "ZERO" ] typeof<IEnumerable<KeyValuePair<int, string>>> "KeyValueEnumerableAdapter`3"
    ()

[<Fact>]
let ``No suitable constructor (dictionary of 'IDictionary' and 'IReadOnlyDictionary', constructor not match)`` () =
    Test ((FakeDictionaryReadOnlyDictionary(dict [ 1, "one"; 0, "ZERO" ] |> ResizeArray))) [ 1, "one"; 0, "ZERO" ] typeof<IEnumerable<KeyValuePair<int, string>>> "KeyValueEnumerableAdapter`3"
    ()
