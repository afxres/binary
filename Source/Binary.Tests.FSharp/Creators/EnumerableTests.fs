namespace Creators

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type segment<'a> = System.ArraySegment<'a>

type vlist<'a> = System.Collections.Generic.List<'a>

type EnumerableTests () =
    let generator = new Generator()

    [<Fact>]
    member __.``IList (Array)`` () =
        let a = [| 1.2; 3.4; 5.6 |] :> IList<float>
        let bytes = generator.ToBytes a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.ToValue<IList<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float segment> value |> ignore
        ()

    [<Fact>]
    member __.``IList (Array Segment)`` () =
        let a = [| 9; 6; 3; |] |> segment
        let bytes = generator.ToBytes a
        Assert.Equal(12, bytes |> Array.length)
        let value = generator.ToValue<IList<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int segment> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyList`` () =
        let a = [ "some"; "times" ] |> vlist :> IReadOnlyList<string>
        let bytes = generator.ToBytes a
        Assert.Equal(4 * 2 + 9, bytes |> Array.length)
        let value = generator.ToValue<IReadOnlyList<string>> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string segment> value |> ignore
        ()

    [<Fact>]
    member __.``ICollection`` () =
        let a = [ 2.2; -4.5; 7.9 ] |> vlist :> ICollection<float>
        let bytes = generator.ToBytes a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.ToValue<ICollection<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float segment> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyCollection`` () =
        let a = [| 13; 31; 131; 1313 |] :> IReadOnlyCollection<int>
        let bytes = generator.ToBytes a
        Assert.Equal(16, bytes |> Array.length)
        let value = generator.ToValue<IReadOnlyCollection<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int segment> value |> ignore
        ()

    [<Fact>]
    member __.``IEnumerable`` () =
        let a = seq { for i in 1..16 do yield sprintf "%x" i }
        let bytes = generator.ToBytes a
        let value = generator.ToValue<string seq> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string segment> value |> ignore
        ()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| typeof<int segment> |]
            yield [| typeof<IEnumerable<string>> |]
            yield [| typeof<IList<int>> |]
            yield [| typeof<IReadOnlyList<string>> |]
            yield [| typeof<ICollection<int>> |]
            yield [| typeof<IReadOnlyCollection<string>> |]
        }

    [<Theory(DisplayName = "Validate Converter Type")>]
    [<MemberData("Data Alpha")>]
    member __.``Validate Converter Type`` (t : Type) =
        let converter = generator.GetConverter t
        let name = converter.GetType().Name
        Assert.StartsWith("IEnumerableConverter`2", name)
        ()
