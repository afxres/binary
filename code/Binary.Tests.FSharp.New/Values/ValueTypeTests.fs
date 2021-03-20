module Values.ValueTypeTests

open Mikodev.Binary
open System
open System.Text
open Xunit

[<Fact>]
let ``Half Converter`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<Half>()
    Assert.StartsWith("NativeEndianConverter`1", converter.GetType().Name)
    ()

[<Fact>]
let ``Rune Converter`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<Rune>()
    Assert.StartsWith("RuneConverter", converter.GetType().Name)
    ()

[<Theory>]
[<InlineData(0x1F600)>]
[<InlineData(0x1F610)>]
let ``Rune`` (data : int) =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<Rune>()
    let rune = Rune data
    let buffer = converter.Encode rune
    let result = converter.Decode buffer
    Assert.Equal(data, result.Value)

    let bufferAuto = Allocator.Invoke(rune, fun allocator data -> converter.EncodeAuto(&allocator, data))
    Assert.Equal<byte>(buffer, bufferAuto)

    let bufferHead = Allocator.Invoke(rune, fun allocator data -> converter.EncodeWithLengthPrefix(&allocator, data))
    let mutable span = ReadOnlySpan bufferHead
    let body = Converter.DecodeWithLengthPrefix &span
    Assert.Equal(0, span.Length)
    Assert.Equal(4, body.Length)
    Assert.Equal<byte>(buffer, body.ToArray())
    ()

[<Theory>]
[<InlineData(0xD800)>]
[<InlineData(0xDFFF)>]
let ``Rune (decode invalid)`` (data : int) =
    let generator = Generator.CreateDefault()
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> generator.Decode<Rune>(generator.Encode data) |> ignore)
    Assert.StartsWith(ArgumentOutOfRangeException().Message, error.Message)
    ()
