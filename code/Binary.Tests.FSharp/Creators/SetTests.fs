module Creators.SetTests

open Mikodev.Binary
open System.Collections.Generic
open Xunit

let generator = Generator.CreateDefault()

[<Fact>]
let ``HashSet Instance`` () =
    let a = [ 3; 6; 9; 0 ] |> HashSet
    let bytes = generator.Encode a
    Assert.Equal(16, bytes |> Array.length)
    let value = generator.Decode<HashSet<int>> bytes
    Assert.Equal<int>(a, value)
    ()

[<Fact>]
let ``ISet Interface`` () =
    let a = seq { for i in 9..16 do yield sprintf "%x" i } |> HashSet :> ISet<string>
    let bytes = generator.Encode a
    let value = generator.Decode<ISet<string>> bytes
    Assert.Equal<string>(a, value)
    Assert.IsType<HashSet<string>> value |> ignore
    ()
