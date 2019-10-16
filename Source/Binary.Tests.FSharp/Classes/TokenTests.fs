module Classes.TokenTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type Packet<'a> = { id : int; data : 'a }

type Person = { name : string; age : int }

let generator = new Generator()

[<Fact>]
let ``As`` () =
    let source = { id = 1; data = { name = "alice"; age = 20 }}
    let buffer = generator.ToBytes source
    let token = generator.AsToken buffer
    Assert.Equal(source.id, token.["id"].As<int>())
    Assert.Equal(source.data.name, token.["data"].["name"].As<string>())
    ()

[<Fact>]
let ``As (via type)`` () =
    let source = { id = 2; data = { name = "bob"; age = 22 }}
    let buffer = generator.ToBytes source
    let token = generator.AsToken buffer
    Assert.Equal(box source.data.name, token.["data"].["name"].As(typeof<string>))
    Assert.Equal(box source.data.age, token.["data"].["age"].As<int>())
    ()

[<Fact>]
let ``As (via type, argument null)`` () =
    let token = generator.AsToken Array.empty<byte>
    let error = Assert.Throws<ArgumentNullException>(fun () -> token.As null |> ignore)
    Assert.Equal("type", error.ParamName)
    ()

[<Fact>]
let ``Index`` () =
    let source = { id = 5; data = { name = "echo"; age = 24 }}
    let buffer = generator.ToBytes source
    let token = generator.AsToken buffer
    let id = token.["id"]
    let name = token.["data"].["name"]
    Assert.Equal(source.id, id.As<int>())
    Assert.Equal(source.data.name, name.As<string>())
    ()

[<Fact>]
let ``Index (mismatch)`` () =
    let source = { id = 6; data = { name = "golf"; age = 18 }}
    let buffer = generator.ToBytes source
    let token = generator.AsToken buffer
    Assert.Equal(source.data, token.["data"].As<Person>())
    Assert.Throws<KeyNotFoundException>(fun () -> token.["item"] |> ignore) |> ignore
    ()

[<Fact>]
let ``As Memory`` () =
    let buffer = [| 32uy..128uy |]
    let origin = new ReadOnlyMemory<byte>(buffer, 8, 48)
    let source = generator.AsToken &origin
    let memory = source.AsMemory()
    Assert.Equal<byte>(origin.ToArray(), memory.ToArray())
    ()

[<Fact>]
let ``As Span`` () =
    let buffer = [| 0uy..100uy |]
    let origin = new ReadOnlyMemory<byte>(buffer, 16, 32)
    let source = generator.AsToken &origin
    let span = source.AsSpan()
    Assert.Equal<byte>(origin.ToArray(), span.ToArray())
    ()

[<Fact>]
let ``Empty Key Only`` () =
    let buffer = generator.ToBytes (dict [ "", box 1.41 ])
    let token = generator.AsToken buffer
    let dictionary = token :> IReadOnlyDictionary<string, Token>
    Assert.Equal(1, dictionary.Count)
    Assert.Equal(1.41, token.[""].As<double>())
    ()

[<Fact>]
let ``Empty Key`` () =
    let buffer = generator.ToBytes (dict [ "a", box 'a'; "", box 2048; "data", box "value" ])
    let token = generator.AsToken buffer
    let dictionary = token :> IReadOnlyDictionary<string, Token>
    Assert.Equal(3, dictionary.Count)
    Assert.Equal(2048, token.[""].As<int>())
    ()

[<Fact>]
let ``To String (debug)`` () =
    let token = generator.AsToken (Array.empty)
    Assert.Equal("Token(Items: 0, Bytes: 0)", token.ToString())
    ()
