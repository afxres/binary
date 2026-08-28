namespace Mikodev.Binary

open Mikodev.Binary.Creators
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderFSharpExtensions =
    [<Extension>]
    [<RequiresDynamicCode("Dynamic code required for binary serialization.")>]
    [<RequiresUnreferencedCode("Public members required for binary serialization.")>]
    static member AddFSharpConverterCreators(builder: IGeneratorBuilder) =
        builder
            .AddConverterCreator(ListConverterCreator())
            .AddConverterCreator(MapConverterCreator())
            .AddConverterCreator(SetConverterCreator())
            .AddConverterCreator(UnionConverterCreator())
