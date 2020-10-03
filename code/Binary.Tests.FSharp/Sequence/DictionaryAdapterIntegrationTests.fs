namespace Sequence

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open Xunit

type SimpleFakeDictionary<'k, 'v>(sequence : ICollection<KeyValuePair<'k, 'v>>) =
    interface IDictionary<'k, 'v> with
        member this.Add(key: 'k, value: 'v): unit = raise (NotImplementedException())

        member this.Add(item: KeyValuePair<'k,'v>): unit = raise (NotImplementedException())

        member this.Clear(): unit = raise (NotImplementedException())

        member this.Contains(item: KeyValuePair<'k,'v>): bool = raise (NotImplementedException())

        member this.ContainsKey(key: 'k): bool = raise (NotImplementedException())

        member this.CopyTo(array: KeyValuePair<'k,'v> [], arrayIndex: int): unit = sequence.CopyTo(array, arrayIndex)

        member this.Count: int = sequence.Count

        member this.GetEnumerator(): IEnumerator<KeyValuePair<'k,'v>> = raise (NotImplementedException())

        member this.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member this.IsReadOnly: bool = raise (NotImplementedException())

        member this.Item with get (key: 'k): 'v = raise (NotImplementedException()) and set (key: 'k) (v: 'v): unit = raise (NotImplementedException())

        member this.Keys: ICollection<'k> = raise (NotImplementedException())

        member this.Remove(key: 'k): bool = raise (NotImplementedException())

        member this.Remove(item: KeyValuePair<'k,'v>): bool = raise (NotImplementedException())

        member this.TryGetValue(key: 'k, value: byref<'v>): bool = raise (NotImplementedException())

        member this.Values: ICollection<'v> =raise (NotImplementedException())

type SimpleFakeReadOnlyDictionary<'k, 'v>(sequence : ICollection<KeyValuePair<'k, 'v>>) =
    interface IReadOnlyDictionary<'k, 'v> with
        member this.ContainsKey(key: 'k): bool = raise (NotImplementedException())

        member this.Count: int = raise (NotImplementedException())

        member this.GetEnumerator(): IEnumerator = raise (NotImplementedException())

        member this.GetEnumerator(): IEnumerator<KeyValuePair<'k,'v>> = sequence.GetEnumerator()

        member this.Item with get (key: 'k): 'v = raise (NotImplementedException())

        member this.Keys: IEnumerable<'k> = raise (NotImplementedException())

        member this.TryGetValue(key: 'k, value: byref<'v>): bool = raise (NotImplementedException())

        member this.Values: IEnumerable<'v> = raise (NotImplementedException())

type DictionaryAdapterIntegrationTests() =
    let generator = Generator.CreateDefault()

    member  __.Test<'t, 'k, 'v when 't : null and 't :> IEnumerable<KeyValuePair<'k, 'v>>> (origin : IEnumerable<KeyValuePair<'k, 'v>>) (item : 't) =
        let converter = generator.GetConverter<'t>()
        Assert.Equal("SequenceConverter`2", converter.GetType().Name)
        let adapter = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(converter)
        Assert.Equal("DictionaryAdapter`3", adapter.GetType().Name)

        let bufferNull = converter.Encode null
        Assert.Equal(0, bufferNull.Length)
        let resultNull = converter.Decode bufferNull
        Assert.Empty resultNull

        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.IsType<Dictionary<'k, 'v>>(box result) |> ignore
        Assert.Equal<KeyValuePair<'k, 'v>>(origin, result)
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'IDictionary' (system 'Dictionary')`` ()=
        let x = [ 1, 2.2; 4, 2.2222 ] |> dict
        me.Test<IDictionary<_, _>, _, _> x (x |> Dictionary<_, _>)
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'IDictionary' (custom 'IDictionary')`` ()=
        let x = [ "a", int 'A'; "b", int 'B' ] |> dict
        me.Test<IDictionary<_, _>, _, _> x (x |> SimpleFakeDictionary<_, _>)
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'IReadOnlyDictionary' (system 'Dictionary')`` ()=
        let x = [ 1, 2.2; 4, 2.2222 ] |> dict
        me.Test<IReadOnlyDictionary<_, _>, _, _> x (x |> Dictionary<_, _>)
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'IReadOnlyDictionary' (custom 'IReadOnlyDictionary')`` ()=
        let x = [ "a", int 'A'; "b", int 'B' ] |> dict
        me.Test<IReadOnlyDictionary<_, _>, _, _> x (x |> SimpleFakeReadOnlyDictionary<_, _>)
        ()
