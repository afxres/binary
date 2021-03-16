namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

[<CompiledName("FSharpListConverterCreator")>]
type internal ListConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            if IsImplementationOf<List<_>> t then
                let itemType = t.GetGenericArguments() |> Array.exactlyOne
                let itemConverter = EnsureHelper.EnsureConverter context itemType
                let converterType = MakeGenericType<ListConverter<_>> itemType
                let converterArguments = [| box itemConverter |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter
            else
                null
