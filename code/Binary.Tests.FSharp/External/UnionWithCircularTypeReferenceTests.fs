module External.UnionWithCircularTypeReferenceTests

open Mikodev.Binary
open Xunit

type Tree =
    | Tip
    | Node of int * Tree * Tree

let nodeV t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(v, _, _) -> v

let nodeL t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(_, l, _) -> l

let nodeR t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(_, _, r) -> r

[<Fact>]
let ``Custom Binary Tree Test`` () =
    let tree = Node(0, Node(1, Node(2, Tip, Tip), Node(3, Tip, Tip)), Node(4, Tip, Tip))
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<Tree>()
    let buffer = converter.Encode tree
    let result = converter.Decode buffer
    Assert.Equal(0, result |> nodeV)
    Assert.Equal(1, result |> nodeL |> nodeV)
    Assert.Equal(2, result |> nodeL |> nodeL |> nodeV)
    Assert.Equal(3, result |> nodeL |> nodeR |> nodeV)
    Assert.Equal(4, result |> nodeR |> nodeV)
    ()
