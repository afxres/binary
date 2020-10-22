namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

[<CompiledName("FSharpListConverterCreator")>]
type internal ListConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>> then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let itemConverter = EnsureHelper.EnsureConverter context itemType
                let converterType = typedefof<ListConverter<_>>.MakeGenericType itemType
                let converterArguments = [| box itemConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
