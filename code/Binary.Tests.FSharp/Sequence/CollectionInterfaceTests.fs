module Sequence.CollectionInterfaceTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type FakeReadOnlySet<'T>(data: ICollection<'T>) =
    interface IReadOnlySet<'T> with
        member __.Contains(_: 'T) : bool = raise (NotSupportedException())

        member __.Count: int = data.Count

        member __.GetEnumerator() : IEnumerator<'T> = data.GetEnumerator()

        member __.GetEnumerator() : System.Collections.IEnumerator = raise (NotSupportedException())

        member __.IsProperSubsetOf(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

        member __.IsProperSupersetOf(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

        member __.IsSubsetOf(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

        member __.IsSupersetOf(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

        member __.Overlaps(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

        member __.SetEquals(_: IEnumerable<'T>) : bool = raise (NotSupportedException())

[<Fact>]
let ``IReadOnlySet`` () =
    let generator = Generator.CreateDefault()
    let data = [| 1; 2; 4 |]
    let set = FakeReadOnlySet(data) :> IReadOnlySet<_>
    let converter = generator.GetConverter set
    let buffer = converter.Encode set
    let result = converter.Decode buffer
    let target = Assert.IsType<HashSet<int>> result
    Assert.Equal<int>(data |> HashSet, target)
    ()

[<Fact>]
let ``IReadOnlySet (null)`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<IReadOnlySet<int>>()
    let buffer = converter.Encode null
    let result = converter.Decode buffer
    let target = Assert.IsType<HashSet<int>> result
    Assert.Equal(0, target.Count)
    ()
