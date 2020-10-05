module Contexts.AllocatorAnchorTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

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

[<Fact>]
let ``Equals (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> AllocatorAnchor().Equals null |> ignore) |> ignore
    ()

[<Fact>]
let ``Get Hash Code (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> AllocatorAnchor().GetHashCode() |> ignore) |> ignore
    ()

[<Fact>]
let ``To String (debug)`` () =
    let mutable allocator = Allocator()
    let anchor = AllocatorHelper.Anchor(&allocator, 4)
    Assert.Equal("AllocatorAnchor(Offset: 0, Length: 4)", anchor.ToString())
    ()
