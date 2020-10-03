namespace Sequence

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Linq
open System.Reflection
open Xunit

type CollectionOfMemory<'T> (memory : ReadOnlyMemory<'T>) =
    member val CopyToCalledCount = 0 with get, set

    interface ICollection<'T> with
        member __.Add(item: 'T): unit = raise (NotImplementedException())

        member __.Clear(): unit = raise (NotImplementedException())

        member __.Contains(item: 'T): bool = raise (NotImplementedException())

        member me.CopyTo(array: 'T [], arrayIndex: int): unit =
            Assert.NotNull array
            Assert.Equal(0, arrayIndex)
            Assert.Equal(memory.Length, array.Length)
            memory.CopyTo(Memory array)
            me.CopyToCalledCount <- me.CopyToCalledCount + 1
            ()

        member __.Count: int = memory.Length

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotImplementedException())

        member __.GetEnumerator(): Collections.IEnumerator = raise (NotImplementedException())

        member __.IsReadOnly: bool = raise (NotImplementedException())

        member __.Remove(item: 'T): bool = raise (NotImplementedException())

type EnumerableAdapterIntegrationTests () =
    let generator = Generator.CreateDefault()

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| Enumerable.Range(0, 8192).ToArray() |]
        yield [| Enumerable.Range(0, 4096).Select(fun x -> x.ToString()).ToArray() |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Collection Encode As 'ICollection' Then Decode As 'IEnumerable'`` (item : 'a array) =
        let memory = ReadOnlyMemory item
        let converter = generator.GetConverter<'a seq>()
        let collection = CollectionOfMemory memory
        Assert.Equal(0, collection.CopyToCalledCount)
        let buffer = converter.Encode collection
        Assert.Equal(1, collection.CopyToCalledCount)
        let result = converter.Decode buffer |> Seq.toArray
        Assert.Equal<'a>(item, result)
        ()

    static member ``Data Bravo`` : (obj array) seq = seq {
        yield [| [| 1; 4; 9; 7; 3 |] |]
        yield [| "error" |> ResizeArray<char> |]
        yield [| seq { for i = 0 to 127 do yield (double i) } |]
        yield [| "Exception" |> Seq.map int64 |> HashSet<_> |]
    }

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Encode Then Decode As 'IEnumerable'`` (item : 'a seq) =
        let converter = generator.GetConverter<'a seq>()
        Assert.Equal("SequenceConverter`2", converter.GetType().Name)
        let adapter = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(converter)
        Assert.Equal("EnumerableAdapter`2", adapter.GetType().Name)

        let bufferNull = converter.Encode null
        Assert.Equal(0, bufferNull.Length)
        let resultNull = converter.Decode bufferNull
        Assert.Empty resultNull

        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.IsType<ArraySegment<'a>>(box result) |> ignore
        Assert.Equal<'a>(item, result)
        ()
