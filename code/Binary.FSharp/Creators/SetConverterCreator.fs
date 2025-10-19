namespace Mikodev.Binary.Creators

open Mikodev.Binary
open Mikodev.Binary.Internal

[<CompiledName("FSharpSetConverterCreator")>]
type internal SetConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            CommonHelper.TryCreateConverter(context, t, typedefof<Set<_>>, typedefof<SetConverter<_>>)
