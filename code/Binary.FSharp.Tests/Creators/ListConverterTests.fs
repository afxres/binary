module Creators.ListConverterTests

open Mikodev.Binary
open System.Linq
open Xunit

let ``List Converter Encode Decode Test`` (source: 'E list) =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<'E list>()
    Assert.Equal("FSharpListConverter`1", converter.GetType().Name)
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal<'E>(source, result)
    ()

[<Fact>]
let ``List Converter For Loop Data Test`` () =
    for i = 0 to 128 do
        let a = Enumerable.Range(0, i) |> Seq.toList
        let b = Enumerable.Range(0, i) |> Seq.map string |> Seq.toList
        Assert.Equal(i, a.Length)
        Assert.Equal(i, b.Length)
        ``List Converter Encode Decode Test`` a
        ``List Converter Encode Decode Test`` b
    ()
