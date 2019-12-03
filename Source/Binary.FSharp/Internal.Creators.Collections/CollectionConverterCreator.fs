namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

type CollectionConverterCreator() =
    static let types = [
        typedefof<List<_>>, typedefof<ListConverter<_>>;
        typedefof<Set<_>>, typedefof<SetConverter<_>>;
        typedefof<Map<_, _>>, typedefof<MapConverter<_, _>>;
    ]

    interface IConverterCreator with
        member __.GetConverter(context, t) =
            let makeConverter (definition : Type) =
                let converterType = definition.MakeGenericType(t.GetGenericArguments())
                let constructor = converterType.GetConstructors() |> Array.exactlyOne
                let itemTypes = constructor.GetParameters() |> Array.map (fun x -> x.ParameterType.GetGenericArguments() |> Array.exactlyOne)
                let itemConverters = itemTypes |> Array.map context.GetConverter
                let converterArguments = itemConverters |> Array.map box
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> Converter

            let definition = if t.IsGenericType then t.GetGenericTypeDefinition() else null
            let definition = types |> List.choose (fun (a, b) -> if a = definition then Some b else None) |> List.tryExactlyOne
            match definition with
            | Some x -> makeConverter x
            | _ -> null
