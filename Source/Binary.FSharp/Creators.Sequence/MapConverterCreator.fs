namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal.Contexts
open System

[<CompiledName("FSharpMapConverterCreator")>]
type internal MapConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Map<_, _>> then
                let itemTypes = t.GetGenericArguments()
                let itemConverters = itemTypes |> Array.map (Validate.GetConverter context)
                let converterType = typedefof<MapConverter<_, _>>.MakeGenericType itemTypes
                let converterArguments = itemConverters |> Array.map box
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
