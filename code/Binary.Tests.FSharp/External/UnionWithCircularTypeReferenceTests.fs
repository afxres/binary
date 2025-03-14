module External.UnionWithCircularTypeReferenceTests

open Mikodev.Binary
open Xunit

type Tree =
    | Tip
    | Node of byte * Tree * Tree

let getV t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(v, _, _) -> v

let getL t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(_, l, _) -> l

let getR t =
    match t with
    | Tip -> raise (exn "bad access")
    | Node(_, _, r) -> r

[<Fact>]
let ``Custom Binary Tree Test`` () =
    let tree = Node(0uy, Node(1uy, Node(2uy, Tip, Tip), Node(3uy, Tip, Tip)), Node(4uy, Tip, Tip))
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<Tree>()
    let buffer = converter.Encode tree
    Assert.Equal(1 + 5 * 3, buffer.Length)
    let result = converter.Decode buffer
    Assert.Equal(0uy, result |> getV)
    Assert.Equal(1uy, result |> getL |> getV)
    Assert.Equal(2uy, result |> getL |> getL |> getV)
    Assert.Equal(3uy, result |> getL |> getR |> getV)
    Assert.Equal(4uy, result |> getR |> getV)
    ()
