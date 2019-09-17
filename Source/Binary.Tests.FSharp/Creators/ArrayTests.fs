module Creators.ArrayTests

open Mikodev.Binary
open System
open Xunit

let generator = new Generator()

[<Fact>]
let ``Array Instance`` () =
    let a : int array = [| 1; 4; 16; |]
    let b : string array = [| "alpha"; "beta"; "release" |]
    let bytesA = generator.ToBytes a
    let bytesB = generator.ToBytes b
    Assert.Equal(12, bytesA.Length)
    Assert.Equal(4 * 3 + 16, bytesB.Length)
    let valueA = generator.ToValue<int array> bytesA
    let valueB = generator.ToValue<string array> bytesB
    Assert.Equal<int>(a, valueA)
    Assert.Equal<string>(b, valueB)
    ()

[<Fact>]
let ``Multidimensional Array`` () =
    let array = Array2D.zeroCreate<int> 2 3
    Assert.Equal(2, array.Rank)
    let _ = Assert.Throws<NotSupportedException>(fun () -> generator.ToBytes array |> ignore)
    ()

[<Fact>]
let ``Array of Arrays`` () =
    let array = [| [| 1; 2|]; [| 5; 7; 9|] |]
    Assert.Equal(1, array.Rank)
    let bytes = generator.ToBytes array
    Assert.Equal(4 * 2 + 4 * 5, bytes |> Array.length)
    let value = generator.ToValue<int [] []> bytes
    Assert.Equal<int []>(array, value)
    ()
