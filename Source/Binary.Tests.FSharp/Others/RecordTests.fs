module Others.RecordTests

open Mikodev.Binary
open System
open Xunit

let generator = new Generator()

type Person = { name : string; age : int }

type PersonDetail = { id : Guid; person : Person; tag : string}

[<Fact>]
let ``Person`` () =
    let person = { name = "coder"; age = 24 }
    let buffer = generator.ToBytes person
    let result : Person = generator.ToValue buffer
    Assert.Equal(person, result)
    ()

[<Fact>]
let ``Person Detail`` () =
    let person = { name = "alice"; age = 20 }
    let detail = { id = Guid.NewGuid(); person = person; tag = "girl" }
    let buffer = generator.ToBytes detail
    let result : PersonDetail = generator.ToValue buffer
    Assert.Equal(detail, result)
    ()

[<Fact>]
let ``Anonymous Record`` () =
    let source = {| key = "sharp"; data = Guid.NewGuid() |}
    let buffer = generator.ToBytes source
    let span = new ReadOnlySpan<byte>(buffer)
    let result = generator.ToValue(&span, source)
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(source, result)
    ()

[<Fact>]
let ``Anonymous Value Record`` () =
    let source = struct {| key = 2048; data = "delta" |}
    let buffer = generator.ToBytes source
    let span = new ReadOnlySpan<byte>(buffer)
    let result = generator.ToValue(&span, source)
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(source, result)
    ()
