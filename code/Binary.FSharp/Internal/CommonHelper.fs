namespace Mikodev.Binary.Internal

open Mikodev.Binary
open System

[<Sealed>]
[<AbstractClass>]
type internal CommonHelper =
    static member GetConverter(context : IGeneratorContext, t : Type) =
        let converter = context.GetConverter t
        let expectedType = typedefof<Converter<_>>.MakeGenericType t
        if isNull (box converter) then
            raise (ArgumentException $"Can not convert null to '{expectedType}'")
        let instanceType = converter.GetType()
        if (expectedType.IsAssignableFrom instanceType = false) then
            raise (ArgumentException $"Can not convert '{instanceType}' to '{expectedType}'")
        converter

    static member GetConverter(context : IGeneratorContext, t : Type, typeDefinition : Type, converterDefinition : Type) =
        assert (converterDefinition.IsGenericTypeDefinition)
        assert (converterDefinition.GetGenericArguments().Length = typeDefinition.GetGenericArguments().Length)
        if t.IsGenericType && t.GetGenericTypeDefinition() = typeDefinition then
            let itemTypes = t.GetGenericArguments()
            let converterType = converterDefinition.MakeGenericType itemTypes
            let converterArguments = itemTypes |> Seq.map (fun x -> CommonHelper.GetConverter(context, x) |> box) |> Seq.toArray
            let converter = Activator.CreateInstance(converterType, converterArguments)
            converter :?> IConverter
        else
            null

    static member GetMethod(t : Type, name : string) =
        let result = t.GetMethod name
        if isNull (box result) then
            raise (MissingMethodException $"Method not found, method name: {name}, type: {t}")
        result
