namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

[<CompiledName("FSharpMapConverterCreator")>]
type internal MapConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if IsImplementationOf<Map<_, _>> t then
                let itemTypes = t.GetGenericArguments()
                let itemConverters = itemTypes |> Array.map (EnsureHelper.EnsureConverter context)
                let converterType = typeof<MapConverter<_, _>>.GetGenericTypeDefinition().MakeGenericType itemTypes
                let converterArguments = itemConverters |> Array.map box
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
