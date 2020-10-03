namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal.Contexts
open System

[<CompiledName("FSharpSetConverterCreator")>]
type internal SetConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Set<_>> then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let itemConverter = Validate.GetConverter context itemType
                let converterType = typedefof<SetConverter<_>>.MakeGenericType itemType
                let converterArguments = [| box itemConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
