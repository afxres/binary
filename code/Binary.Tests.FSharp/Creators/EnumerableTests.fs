namespace Creators

open Mikodev.Binary
open System.Collections.Generic
open Xunit

type segment<'a> = System.ArraySegment<'a>

type vlist<'a> = System.Collections.Generic.List<'a>

type EnumerableTests () =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``IList (Array)`` () =
        let a = [| 1.2; 3.4; 5.6 |] :> IList<float>
        let bytes = generator.Encode a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.Decode<IList<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float array> value |> ignore
        ()

    [<Fact>]
    member __.``IList (Array Segment)`` () =
        let a = [| 9; 6; 3; |] |> segment
        let bytes = generator.Encode a
        Assert.Equal(12, bytes |> Array.length)
        let value = generator.Decode<IList<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int array> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyList`` () =
        let a = [ "some"; "times" ] |> vlist :> IReadOnlyList<string>
        let bytes = generator.Encode a
        Assert.Equal(1 * 2 + 9, bytes |> Array.length)
        let value = generator.Decode<IReadOnlyList<string>> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string segment> value |> ignore
        ()

    [<Fact>]
    member __.``ICollection`` () =
        let a = [ 2.2; -4.5; 7.9 ] |> vlist :> ICollection<float>
        let bytes = generator.Encode a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.Decode<ICollection<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float array> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyCollection`` () =
        let a = [| 13; 31; 131; 1313 |] :> IReadOnlyCollection<int>
        let bytes = generator.Encode a
        Assert.Equal(16, bytes |> Array.length)
        let value = generator.Decode<IReadOnlyCollection<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int array> value |> ignore
        ()

    [<Fact>]
    member __.``IEnumerable`` () =
        let a = seq { for i in 1..13 do yield sprintf "%x" i }
        let bytes = generator.Encode a
        let value = generator.Decode<string seq> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string segment> value |> ignore
        ()
