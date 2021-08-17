namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal

[<CompiledName("FSharpSetConverterCreator")>]
type internal SetConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            CommonHelper.GetConverter(context, t, typedefof<Set<_>>, typedefof<SetConverter<_>>)
