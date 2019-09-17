module Creators.SetTests

open Mikodev.Binary
open System.Collections.Generic
open Xunit

let generator = new Generator()

[<Fact>]
let ``HashSet Instance`` () =
    let a = [ 3; 6; 9; 0 ] |> HashSet
    let bytes = generator.ToBytes a
    Assert.Equal(16, bytes |> Array.length)
    let value = generator.ToValue<HashSet<int>> bytes
    Assert.Equal<int>(a, value)
    ()

[<Fact>]
let ``ISet Interface`` () =
    let a = seq { for i in 9..16 do yield sprintf "%x" i } |> HashSet :> ISet<string>
    let bytes = generator.ToBytes a
    let value = generator.ToValue<ISet<string>> bytes
    Assert.Equal<string>(a, value)
    Assert.IsType<HashSet<string>> value |> ignore
    ()
