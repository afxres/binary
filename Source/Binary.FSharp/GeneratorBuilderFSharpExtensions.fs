namespace Mikodev.Binary

open Mikodev.Binary.Internal.Creators
open Mikodev.Binary.Internal.Creators.Collections
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderFSharpExtensions =
    [<Extension>]
    static member AddFSharpConverterCreators(builder : IGeneratorBuilder) =
        builder
            .AddConverterCreator(UnionConverterCreator())
            .AddConverterCreator(ListConverterCreator())
            .AddConverterCreator(MapConverterCreator())
            .AddConverterCreator(SetConverterCreator())
