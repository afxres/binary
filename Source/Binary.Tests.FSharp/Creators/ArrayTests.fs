module Creators.ArrayTests

open Mikodev.Binary
open System
open Xunit

let generator = new Generator()

[<Fact>]
let ``Array Instance`` () =
    let a : int array = [| 1; 4; 16; |]
    let b : string array = [| "alpha"; "beta"; "release" |]
    let bytesA = generator.Encode a
    let bytesB = generator.Encode b
    Assert.Equal(12, bytesA.Length)
    Assert.Equal(1 * 3 + 5 + 4 + 7, bytesB.Length)
    let valueA = generator.Decode<int array> bytesA
    let valueB = generator.Decode<string array> bytesB
    Assert.Equal<int>(a, valueA)
    Assert.Equal<string>(b, valueB)
    ()

[<Fact>]
let ``Multidimensional Array`` () =
    let array = Array2D.zeroCreate<int> 2 3
    Assert.Equal(2, array.Rank)
    let _ = Assert.Throws<NotSupportedException>(fun () -> generator.Encode array |> ignore)
    ()

[<Fact>]
let ``Array of Arrays`` () =
    let array = [| [| 1; 2|]; [| 5; 7; 9|] |]
    Assert.Equal(1, array.Rank)
    let bytes = generator.Encode array
    Assert.Equal(1 * 2 + 4 * 5, bytes |> Array.length)
    let value = generator.Decode<int [] []> bytes
    Assert.Equal<int []>(array, value)
    ()
