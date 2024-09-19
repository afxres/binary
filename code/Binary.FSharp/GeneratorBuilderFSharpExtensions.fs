﻿namespace Mikodev.Binary

open Mikodev.Binary.Creators
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices

[<Extension>]
type GeneratorBuilderFSharpExtensions =
    [<Extension>]
    [<RequiresDynamicCode("Requires dynamic code for binary serialization.")>]
    [<RequiresUnreferencedCode("Requires public members for binary serialization.")>]
    static member AddFSharpConverterCreators(builder : IGeneratorBuilder) =
        builder
            .AddConverterCreator(ListConverterCreator())
            .AddConverterCreator(MapConverterCreator())
            .AddConverterCreator(SetConverterCreator())
            .AddConverterCreator(UnionConverterCreator())
