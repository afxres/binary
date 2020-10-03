namespace Sequence

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Linq
open System.Reflection
open Xunit

type SimpleFakeSet<'T>(sequence : ICollection<'T>) =
    interface ISet<'T> with
        member __.Add(item: 'T): bool = raise (NotImplementedException())

        member __.Add(item: 'T): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: 'T): bool = raise (NotImplementedException())

        member __.CopyTo(array: 'T [], arrayIndex: int): unit = sequence.CopyTo(array, arrayIndex)

        member __.Count: int = sequence.Count

        member __.ExceptWith(other: IEnumerable<'T>): unit = raise (NotImplementedException())

        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotImplementedException())

        member __.IntersectWith(other: IEnumerable<'T>): unit = raise (NotImplementedException())

        member __.IsProperSubsetOf(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.IsProperSupersetOf(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.IsSubsetOf(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.IsSupersetOf(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.Overlaps(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.Remove(item: 'T): bool = raise (NotImplementedException())

        member __.SetEquals(other: IEnumerable<'T>): bool = raise (NotImplementedException())

        member __.SymmetricExceptWith(other: IEnumerable<'T>): unit = raise (NotImplementedException())

        member __.UnionWith(other: IEnumerable<'T>): unit = raise (NotImplementedException())

type SetAdapterIntegrationTests() =
    let generator = Generator.CreateDefault()

    member __.Test<'T, 'E when 'T : null and 'T :> IEnumerable<'E>> (origin : IEnumerable<'E>) (item : 'T) =
        let converter = generator.GetConverter<'T>()
        Assert.Equal("SequenceConverter`2", converter.GetType().Name)
        let adapter = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(converter)
        Assert.Equal("SetAdapter`2", adapter.GetType().Name)

        let bufferNull = converter.Encode null
        Assert.Equal(0, bufferNull.Length)
        let resultNull = converter.Decode bufferNull
        Assert.Empty resultNull

        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.IsType<HashSet<'E>>(box result) |> ignore
        Assert.Equal<'E>(origin, result)
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'ISet' (system 'HashSet', count from 0 to 256)`` () =
        for i = 0 to 256 do
            let x = Enumerable.Range(0, i) |> HashSet
            me.Test<ISet<_>, _> x x
        ()

    [<Fact>]
    member me.``Encode Then Decode As 'ISet' (custom 'ISet', count from 0 to 256)`` () =
        for i = 0 to 256 do
            let x = Enumerable.Range(0, i) |> HashSet
            me.Test<ISet<_>, _> x (x |> SimpleFakeSet)
        ()
