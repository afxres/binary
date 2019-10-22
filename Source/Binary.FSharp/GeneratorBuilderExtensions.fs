namespace Mikodev.Binary

open Mikodev.Binary.Creators
open Mikodev.Binary.Creators.Collections
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderExtensions =
    [<Extension>]
    static member AddFSharpConverterCreators(builder : IGeneratorBuilder) =
        builder
            .AddConverterCreator(UnionConverterCreator())
            .AddConverterCreator(CollectionConverterCreator())
