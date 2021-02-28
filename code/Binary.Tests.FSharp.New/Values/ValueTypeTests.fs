module Values.ValueTypeTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``Half Converter`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<Half>()
    Assert.StartsWith("NativeEndianConverter`1", converter.GetType().Name)
    ()
