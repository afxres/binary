namespace Creators

open Mikodev.Binary
open System
open Xunit

type UnionConverterCreatorTests() =
    [<Fact>]
    member __.``Get Converter Of FSharp List`` () =
        let creatorType =
            typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes()
            |> Array.filter (fun x -> x.Name = "UnionConverterCreator")
            |> Array.exactlyOne
        let creator = Activator.CreateInstance creatorType :?> IConverterCreator
        let context = { new IGeneratorContext with member __.GetConverter _ = raise (NotSupportedException()) }
        let types = [| typeof<int list>; typeof<string list> |]
        Assert.All(types, fun x -> Assert.StartsWith("FSharpList`1", x.Name))
        let results = types |> Array.map (fun x -> creator.GetConverter(context, x))
        Assert.All(results, fun x -> Assert.Null x)
        ()
