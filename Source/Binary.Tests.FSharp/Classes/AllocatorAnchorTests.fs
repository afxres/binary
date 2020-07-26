module Classes.AllocatorAnchorTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let outofrange = ArgumentOutOfRangeException().Message

[<Fact>]
let ``Public Members`` () =
    let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "AllocatorAnchor") |> Array.exactlyOne
    let constructors = t.GetConstructors()
    let properties = t.GetProperties()
    let members = t.GetMembers()
    Assert.Empty(constructors)
    Assert.Empty(properties)
    Assert.Equal<string>(members |> Seq.map (fun x -> x.Name) |> HashSet, [| "Equals"; "GetHashCode"; "ToString"; "GetType" |] |> HashSet)
    ()
