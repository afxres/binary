module Implementations.EnumerableAdapterTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

type CollectionA<'T> (count : int) =
    interface ICollection<'T> with
        member __.Add(item: 'T): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: 'T): bool = raise (NotImplementedException())

        member __.CopyTo(array: 'T [], arrayIndex: int): unit = raise (NotSupportedException("Collection A, Copy To"))

        member __.Count: int = count

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException("Collection A, Get Enumerator"))

        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.Remove(item: 'T): bool = raise (NotImplementedException())

type CollectionB<'T> (count : int) =
    member __.ToArray() : 'T array = raise (NotSupportedException("Collection B, To Array"))

    interface IReadOnlyCollection<'T> with
        member __.Count: int = count

        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException("Collection B, Get Enumerator"))

type CollectionC<'T> (count : int) =
    member private __.ToArray() : 'T array = raise (NotImplementedException())

    interface IReadOnlyCollection<'T> with
        member __.Count: int = count

        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException("Collection C, Get Enumerator"))

type CollectionD<'T> (count : int) =
    interface IEnumerable<'T> with
        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException("Collection D, Get Enumerator"))

    interface ICollection<'T> with
        member __.Add(item: 'T): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: 'T): bool = raise (NotImplementedException())

        member __.CopyTo(array: 'T [], arrayIndex: int): unit = raise (NotSupportedException("Collection D, Copy To"))

        member __.Count: int = count

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.Remove(item: 'T): bool = raise (NotImplementedException())

    interface IReadOnlyCollection<'T> with
        member __.Count = raise (NotImplementedException())

type CollectionE<'T> () =
    static member ToArray() : 'T array = raise (NotImplementedException())

    interface IEnumerable<'T> with
        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException("Collection E, Get Enumerator"))

type CollectionF<'T> () =
    member __.ToArray() : 'T array = raise (NotSupportedException("Collection F, To Array"))

    interface IEnumerable<'T> with
        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotImplementedException())

let generator = Generator.CreateDefault()

let test (collection : 'a) (except : string) =
    let converter = generator.GetConverter(anonymous = collection)
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Encode collection |> ignore)
    Assert.Equal(except, error.Message)
    ()

let functor<'a> (isArrayNull : bool) =
    let converter = generator.GetConverter<'a> ()
    let flags = BindingFlags.Instance ||| BindingFlags.NonPublic
    let adapter = converter.GetType().GetField("adapter", flags).GetValue(converter)
    Assert.NotNull adapter
    let array = adapter.GetType().GetField("array", flags).GetValue(adapter)
    Assert.Equal(isArrayNull, isNull array)
    ()

let testEnumerableCount (collection : 'a seq) (expectedCount : int) =
    let converter = generator.GetConverter<'a>()
    let adapterGenericDefinition = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "EnumerableAdapter`2") |> Array.exactlyOne
    let adapterType = adapterGenericDefinition.MakeGenericType(typeof<'a seq>, typeof<'a>)
    let adapter = Activator.CreateInstance(adapterType, [| box converter |])
    let countMethod = adapterType.GetMethod("Count")
    let count = countMethod.Invoke(adapter, [| box collection |]) |> unbox<int>
    Assert.Equal(expectedCount, count)
    ()

let testDictionaryCount (collection : KeyValuePair<'a, 'b> seq) (expectedCount : int) =
    let initConverter = generator.GetConverter<'a>()
    let tailConverter = generator.GetConverter<'b>()
    let itemLength = let list = [| initConverter.Length; tailConverter.Length |] in if Array.contains 0 list then 0 else Array.sum list
    let adapterGenericDefinition = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "DictionaryAdapter`3") |> Array.exactlyOne
    let adapterType = adapterGenericDefinition.MakeGenericType(typeof<KeyValuePair<'a, 'b> seq>, typeof<'a>, typeof<'b>)
    let adapter = Activator.CreateInstance(adapterType, [| box initConverter; box tailConverter; box itemLength |])
    let countMethod = adapterType.GetMethod("Count")
    let count = countMethod.Invoke(adapter, [| box collection |]) |> unbox<int>
    Assert.Equal(expectedCount, count)
    ()

[<Fact>]
let ``Collection Encode (count 8, copy to)`` () = test (CollectionA<int> 8) "Collection A, Copy To"

[<Fact>]
let ``Collection Adapter Functor`` () = functor<CollectionA<int>> true

[<Fact>]
let ``Read Only Collection With To Array Method Encode (count 8, to array)`` () = test (CollectionB<string> 8) "Collection B, To Array"

[<Fact>]
let ``Read Only Collection With To Array Method Adapter Functor`` () = functor<CollectionB<string>> false

[<Fact>]
let ``Read Only Collection With Private To Array Method Encode (count 8, use enumerator)`` () = test (CollectionC<int> 8) "Collection C, Get Enumerator"

[<Fact>]
let ``Read Only Collection With Private To Array Method Adapter Functor`` () = functor<CollectionC<string>> true

[<Fact>]
let ``Collection With Multiple Interfaces (count 8, copy to)`` () = test (CollectionD<string> 8) "Collection D, Copy To"

[<Fact>]
let ``Collection With Multiple Interfaces Adapter Functor`` () = functor<CollectionD<int>> true

[<Fact>]
let ``Enumerable With Static To Array Method Encode (use enumerator)`` () = test (CollectionE<int> ()) "Collection E, Get Enumerator"

[<Fact>]
let ``Enumerable With Static To Array Method Functor`` () = functor<CollectionE<int>> true

[<Fact>]
let ``Enumerable With To Array Method Encode (to array)`` () = test (CollectionF<int> ()) "Collection F, To Array"

[<Fact>]
let ``Enumerable With To Array Method Functor`` () = functor<CollectionF<int>> false

[<Fact>]
let ``Collection Count (enumerable adapter)`` () = testEnumerableCount (CollectionA<int> 16) 16

[<Fact>]
let ``Collection Count (dictionary adapter)`` () = testDictionaryCount (CollectionA<KeyValuePair<int, int>> 33) 33

[<Fact>]
let ``Read Only Collection Count (enumerable adapter)`` () = testEnumerableCount (CollectionB<string> 9) 9

[<Fact>]
let ``Read Only Collection Count (dictionary adapter)`` () = testDictionaryCount (CollectionB<KeyValuePair<int, string>> 13) 13

[<Fact>]
let ``Collection With Multiple Interfaces Count (enumerable adapter)`` () = testEnumerableCount (CollectionD<int> 11) 11

[<Fact>]
let ``Collection With Multiple Interfaces Count (dictionary adapter)`` () = testDictionaryCount (CollectionD<KeyValuePair<string, int>> 17) 17

[<Fact>]
let ``Enumerable Count (enumerable adapter)`` () = testEnumerableCount (Seq.empty<string>) -1

[<Fact>]
let ``Enumerable Count (dictionary adapter)`` () = testDictionaryCount (Seq.empty<KeyValuePair<string, int>>) -1

[<Fact>]
let ``Enumerable Count (enumerable adapter, null)`` () = testEnumerableCount (Unchecked.defaultof<int seq>) 0

[<Fact>]
let ``Enumerable Count (dictionary adapter, null)`` () = testDictionaryCount (Unchecked.defaultof<KeyValuePair<int, string> seq>) 0
