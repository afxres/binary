namespace Mikodev.Binary.Creators

open Mikodev.Binary
open Mikodev.Binary.Internal

[<CompiledName("FSharpListConverterCreator")>]
type internal ListConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            CommonHelper.TryCreateConverter(context, t, typedefof<_ list>, typedefof<ListConverter<_>>)
