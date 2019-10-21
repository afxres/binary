namespace Mikodev.Binary

open Mikodev.Binary.Creators.Collections
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderExtensions =
    [<Extension>]
    static member FSharpConverterCreators(builder : IGeneratorBuilder) =
        builder.AddConverterCreator(CollectionConverterCreator())
    