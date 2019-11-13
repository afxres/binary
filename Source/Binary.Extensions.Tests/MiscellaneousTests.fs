module MiscellaneousTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``Add Converter Creators`` () =
    let builderType = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorBuilder") |> Array.exactlyOne
    let builder = Activator.CreateInstance(builderType) :?> IGeneratorBuilder
    Assert.Equal("GeneratorBuilder(Converters: 0, Creators: 0)", builder.ToString())
    builder.AddExtensionConverterCreators() |> ignore
    Assert.Equal("GeneratorBuilder(Converters: 0, Creators: 3)", builder.ToString())
    ()
