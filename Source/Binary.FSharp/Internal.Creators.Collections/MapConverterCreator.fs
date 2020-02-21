namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpMapConverterCreator")>]
type MapConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Map<_, _>> then
                let itemTypes = t.GetGenericArguments()
                let itemConverters = itemTypes |> Array.map context.GetConverter
                let converterType = typedefof<MapConverter<_, _>>.MakeGenericType itemTypes
                let converterArguments = itemConverters |> Array.map box
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> Converter
            else
                null
