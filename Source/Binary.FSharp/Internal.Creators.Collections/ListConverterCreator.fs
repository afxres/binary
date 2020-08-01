namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpListConverterCreator")>]
type internal ListConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>> then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let itemConverter = context.GetConverter itemType
                let converterType = typedefof<ListConverter<_>>.MakeGenericType itemType
                let converterArguments = [| box itemConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
