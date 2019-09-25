namespace ArrayLike

open Mikodev.Binary
open System
open System.Runtime.InteropServices
open Xunit
open System.Linq

type ArrayLikeTests () =
    let generator = new Generator()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| [| 1; 2 |]; 2 |]
            yield [| [| 1.1; 2.2; 3.3; 4.4; 5.5; 6.6 |]; 6 |]
            yield [| [| "alpha" |]; 4 |]
            yield [| [| "a"; "b"; "c"; "d"; "e" |]; 8 |]
            yield [| Enumerable.Range(0, 48) |> Seq.map (sprintf "%2x") |> Seq.toArray; 64 |]
            yield [| Enumerable.Range(0, 192) |> Seq.map (sprintf "%2d") |> Seq.toArray; 256 |]
        }

    [<Theory(DisplayName = "Memory")>]
    [<MemberData("Data Alpha")>]
    member __.``Memory`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<Memory<'a>>()
        let buffer = converter.ToBytes(Memory item)
        let result = converter.ToValue buffer
        Assert.Equal<'a>(item, result.ToArray())
        let (flag, data) = MemoryMarshal.TryGetArray(Memory.op_Implicit result)
        Assert.True(flag)
        Assert.Equal(capacity, data.Array.Length)
        ()

    [<Theory(DisplayName = "ReadOnlyMemory")>]
    [<MemberData("Data Alpha")>]
    member __.``ReadOnlyMemory`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<ReadOnlyMemory<'a>>()
        let buffer = converter.ToBytes(ReadOnlyMemory item)
        let result = converter.ToValue buffer
        Assert.Equal<'a>(item, result.ToArray())
        let (flag, data) = MemoryMarshal.TryGetArray(result)
        Assert.True(flag)
        Assert.Equal(capacity, data.Array.Length)
        ()

    [<Theory(DisplayName = "ArraySegment")>]
    [<MemberData("Data Alpha")>]
    member __.``ArraySegment`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<ArraySegment<'a>>()
        let buffer = converter.ToBytes(ArraySegment item)
        let result = converter.ToValue buffer
        Assert.Equal<'a>(item, result)
        Assert.Equal(capacity, result.Array.Length)
        ()
