namespace ArrayLike

open Mikodev.Binary
open System
open System.Linq
open System.Runtime.InteropServices
open Xunit

type ArrayLikeTests () =
    let generator = Generator.CreateDefault()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| [| 1; 2 |]; 2 |]
            yield [| [| 1.1; 2.2; 3.3; 4.4; 5.5; 6.6 |]; 6 |]
            yield [| [| "alpha" |]; 8 |]
            yield [| [| "a"; "b"; "c"; "d"; "e" |]; 8 |]
            yield [| Enumerable.Range(0, 48) |> Seq.map (sprintf "%2x") |> Seq.toArray; 64 |]
            yield [| Enumerable.Range(0, 192) |> Seq.map (sprintf "%2d") |> Seq.toArray; 256 |]
        }

    [<Theory(DisplayName = "Memory")>]
    [<MemberData("Data Alpha")>]
    member __.``Memory`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<Memory<'a>>()
        let buffer = converter.Encode(Memory item)
        let result = converter.Decode buffer
        Assert.Equal<'a>(item, result.ToArray())
        let (flag, data) = MemoryMarshal.TryGetArray(Memory.op_Implicit result)
        Assert.True(flag)
        Assert.Equal(capacity, data.Array.Length)
        ()

    [<Theory(DisplayName = "ReadOnlyMemory")>]
    [<MemberData("Data Alpha")>]
    member __.``ReadOnlyMemory`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<ReadOnlyMemory<'a>>()
        let buffer = converter.Encode(ReadOnlyMemory item)
        let result = converter.Decode buffer
        Assert.Equal<'a>(item, result.ToArray())
        let (flag, data) = MemoryMarshal.TryGetArray(result)
        Assert.True(flag)
        Assert.Equal(capacity, data.Array.Length)
        ()

    [<Theory(DisplayName = "ArraySegment")>]
    [<MemberData("Data Alpha")>]
    member __.``ArraySegment`` (item : 'a array, capacity : int) =
        let converter = generator.GetConverter<ArraySegment<'a>>()
        let buffer = converter.Encode(ArraySegment item)
        let result = converter.Decode buffer
        Assert.Equal<'a>(item, result)
        Assert.Equal(capacity, result.Array.Length)
        ()

    static member ``Data Bravo`` : (obj array) seq =
        seq {
            yield [| typeof<ArraySegment<int>>; "ArrayLikeConverter`2" |]
            yield [| typeof<Memory<int>>; "ArrayLikeConverter`2" |]
            yield [| typeof<ReadOnlyMemory<string>>; "ArrayLikeConverter`2" |]
        }

    [<Theory(DisplayName = "Validate Converter Type")>]
    [<MemberData("Data Bravo")>]
    member __.``Validate Converter Type`` (t : Type, starts : string) =
        let converter = generator.GetConverter t
        let name = converter.GetType().Name
        Assert.StartsWith(starts, name)
        ()

    static member ``Data Slice`` : (obj array) seq =
        seq {
            yield [| [| 1; 2; 3; 4; 5 |]; 1; 2 |]
            yield [| [| "a"; "bb"; "ccc"; "0"; "1"; "-1" |]; 2; 3 |]
        }

    [<Theory>]
    [<MemberData("Data Slice")>]
    member __.``Memory Slice`` (item : 'a array, offset : int, length : int) =
        let converter = generator.GetConverter<Memory<'a>>()
        let source = Memory<'a>(item, offset, length)
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.Equal<'a>(source.ToArray(), result.ToArray())
        ()

    [<Theory>]
    [<MemberData("Data Slice")>]
    member __.``ReadOnlyMemory Slice`` (item : 'a array, offset : int, length : int) =
        let converter = generator.GetConverter<ReadOnlyMemory<'a>>()
        let source = ReadOnlyMemory<'a>(item, offset, length)
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.Equal<'a>(source.ToArray(), result.ToArray())
        ()

    [<Theory>]
    [<MemberData("Data Slice")>]
    member __.``ArraySegment Slice`` (item : 'a array, offset : int, length : int) =
        let converter = generator.GetConverter<ArraySegment<'a>>()
        let source = ArraySegment<'a>(item, offset, length)
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.Equal<'a>(source.ToArray(), result.ToArray())
        ()
