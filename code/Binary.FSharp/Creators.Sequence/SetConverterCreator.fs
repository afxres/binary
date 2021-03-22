namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

[<CompiledName("FSharpSetConverterCreator")>]
type internal SetConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if IsImplementationOf<Set<_>> t then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let converterType = MakeGenericType<SetConverter<_>> itemType
                let converterArguments = [| CommonHelper.GetConverter(context, itemType) |> box |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
