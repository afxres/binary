namespace Sequence

open Mikodev.Binary
open System.Collections.Immutable
open Xunit

type ImmutableCollectionTests() =
    let generator = Generator.CreateDefault()

    let TestNull (expected : 'T) =
        let converter = generator.GetConverter<'T>()
        let buffer = converter.Encode null
        Assert.Empty buffer
        let result = converter.Decode buffer
        Assert.True(obj.ReferenceEquals(result, expected))
        ()

    let Test (item : 'a when 'a :> 'e seq) =
        let converter = generator.GetConverter<'a>()
        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.Equal<'e>(item, result)
        ()

    [<Fact>]
    member __.``Immutable List Null`` () =
        TestNull ImmutableList<int>.Empty
        TestNull ImmutableList<string>.Empty
        ()

    [<Fact>]
    member __.``Immutable List`` ()=
        Test (ImmutableList.CreateRange [| 1; 5 |])
        Test (ImmutableList.CreateRange [| "immutable"; "list"; "collection"; "readonly" |])
        ()
