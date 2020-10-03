module External.RecordTests

open Mikodev.Binary
open System
open Xunit

let generator = Generator.CreateDefault()

type Person = { name : string; age : int }

type PersonDetail = { id : Guid; person : Person; tag : string}

[<Fact>]
let ``Person`` () =
    let person = { name = "coder"; age = 24 }
    let buffer = generator.Encode person
    let result : Person = generator.Decode buffer
    Assert.Equal(person, result)
    ()

[<Fact>]
let ``Person Detail`` () =
    let person = { name = "alice"; age = 20 }
    let detail = { id = Guid.NewGuid(); person = person; tag = "girl" }
    let buffer = generator.Encode detail
    let result : PersonDetail = generator.Decode buffer
    Assert.Equal(detail, result)
    ()

[<Fact>]
let ``Anonymous Class Record`` () =
    let source = {| key = "sharp"; data = Guid.NewGuid() |}
    let buffer = generator.Encode source
    let span = new ReadOnlySpan<byte>(buffer)
    let result = generator.Decode(span, source)
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(source, result)
    ()

[<Fact>]
let ``Anonymous Value Record`` () =
    let source = struct {| key = 2048; data = "delta" |}
    let buffer = generator.Encode source
    let span = new ReadOnlySpan<byte>(buffer)
    let result = generator.Decode(span, source)
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(source, result)
    ()
