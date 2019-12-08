namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

type SetConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Set<_>> then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let itemConverter = context.GetConverter itemType
                let converterType = typedefof<SetConverter<_>>.MakeGenericType itemType
                let converterArguments = [| box itemConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> Converter
            else
                null
