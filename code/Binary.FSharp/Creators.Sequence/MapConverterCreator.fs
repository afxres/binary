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
                let converterType = typeof<MapConverter<_, _>>.GetGenericTypeDefinition().MakeGenericType itemTypes
                let converterArguments = itemTypes |> Seq.map (EnsureHelper.EnsureConverter context >> box) |> Seq.toArray
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
