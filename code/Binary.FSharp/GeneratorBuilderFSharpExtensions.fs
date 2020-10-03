namespace Mikodev.Binary

open System
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderFSharpExtensions =
    [<Extension>]
    static member AddFSharpConverterCreators(builder : IGeneratorBuilder) =
        let creators =
            typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes()
                |> Seq.filter (fun x -> typeof<IConverterCreator>.IsAssignableFrom x)
                |> Seq.map (fun x -> Activator.CreateInstance x :?> IConverterCreator)
                |> Seq.toList
        for i in creators do
            builder.AddConverterCreator i |> ignore
        builder
