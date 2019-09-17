module Creators.ListTests

open Mikodev.Binary
open System.Collections.Generic
open Xunit

type vlist<'a> = System.Collections.Generic.List<'a>

let generator = new Generator()

[<Fact>]
let ``List`` () =
    let a = [ 1; 4; 7 ] |> vlist
    let b = [ "lazy"; "dog"; "quick"; "fox" ] |> vlist
    let bytesA = generator.ToBytes a
    let bytesB = generator.ToBytes b
    Assert.Equal(12, bytesA |> Array.length)
    Assert.Equal(4 * 4 + 15, bytesB |> Array.length)
    let valueA = generator.ToValue<int vlist> bytesA
    let valueB = generator.ToValue<string vlist> bytesB
    Assert.Equal<int>(a, valueA)
    Assert.Equal<string>(b, valueB)
    ()

[<Fact>]
let ``List (null and empty)`` () =
    let a = Array.empty<int> |> vlist
    let b = null : string vlist
    let bytesA = generator.ToBytes a
    let bytesB = generator.ToBytes b
    Assert.NotNull(bytesA)
    Assert.NotNull(bytesB)
    Assert.Empty(bytesA)
    Assert.Empty(bytesB)
    let valueA = generator.ToValue<int vlist> bytesA
    let valueB = generator.ToValue<string vlist> bytesB
    Assert.Empty(valueA)
    Assert.Empty(valueB)
    ()

[<Fact>]
let ``IList (Array)`` () =
    let a = [| 1.2; 3.4; 5.6 |] :> IList<float>
    let bytes = generator.ToBytes a
    Assert.Equal(24, bytes |> Array.length)
    let value = generator.ToValue<IList<float>> bytes
    Assert.Equal<float>(a, value)
    Assert.IsType<float array> value |> ignore
    ()

[<Fact>]
let ``IList (Array Segment)`` () =
    let a = [| 9; 6; 3; |] |> System.ArraySegment
    let bytes = generator.ToBytes a
    Assert.Equal(12, bytes |> Array.length)
    let value = generator.ToValue<IList<int>> bytes
    Assert.Equal<int>(a, value)
    Assert.IsType<int array> value |> ignore
    ()

[<Fact>]
let ``IReadOnlyList`` () =
    let a = [ "some"; "times" ] |> vlist :> IReadOnlyList<string>
    let bytes = generator.ToBytes a
    Assert.Equal(4 * 2 + 9, bytes |> Array.length)
    let value = generator.ToValue<IReadOnlyList<string>> bytes
    Assert.Equal<string>(a, value)
    Assert.IsType<string vlist> value |> ignore
    ()

[<Fact>]
let ``ICollection`` () =
    let a = [ 2.2; -4.5; 7.9 ] |> vlist :> ICollection<float>
    let bytes = generator.ToBytes a
    Assert.Equal(24, bytes |> Array.length)
    let value = generator.ToValue<ICollection<float>> bytes
    Assert.Equal<float>(a, value)
    Assert.IsType<float array> value |> ignore
    ()

[<Fact>]
let ``IReadOnlyCollection`` () =
    let a = [| 13; 31; 131; 1313 |] :> IReadOnlyCollection<int>
    let bytes = generator.ToBytes a
    Assert.Equal(16, bytes |> Array.length)
    let value = generator.ToValue<IReadOnlyCollection<int>> bytes
    Assert.Equal<int>(a, value)
    Assert.IsType<int array> value |> ignore
    ()

[<Fact>]
let ``IEnumerable`` () =
    let a = seq { for i in 1..16 do yield sprintf "%x" i }
    let bytes = generator.ToBytes a
    let value = generator.ToValue<string seq> bytes
    Assert.Equal<string>(a, value)
    Assert.IsType<string vlist> value |> ignore
    ()
