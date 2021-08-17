﻿namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal

[<CompiledName("FSharpMapConverterCreator")>]
type internal MapConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            CommonHelper.GetConverter(context, t, typedefof<Map<_, _>>, typedefof<MapConverter<_, _>>)
