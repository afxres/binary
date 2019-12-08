namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System
open System.Collections.Generic

type MapConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Map<_, _>> then
                let itemTypes = t.GetGenericArguments()
                let pairConverter = context.GetConverter (typedefof<KeyValuePair<_, _>>.MakeGenericType itemTypes)
                let converterType = typedefof<MapConverter<_, _>>.MakeGenericType itemTypes
                let converterArguments = [| box pairConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> Converter
            else
                null
