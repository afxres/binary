namespace Mikodev.Binary

open Mikodev.Binary.Creators
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderFSharpExtensions =
    [<Extension>]
    static member AddFSharpConverterCreators(builder : IGeneratorBuilder) =
        builder
            .AddConverterCreator(ListConverterCreator())
            .AddConverterCreator(MapConverterCreator())
            .AddConverterCreator(SetConverterCreator())
            .AddConverterCreator(UnionConverterCreator())
