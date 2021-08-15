namespace Mikodev.Binary.Internal

open Mikodev.Binary
open System

[<Sealed>]
[<AbstractClass>]
type internal CommonHelper =
    static member GetConverter(context : IGeneratorContext, t : Type) =
        let converter = context.GetConverter t
        let expectedType = MakeGenericType<Converter<_>> t
        if isNull (box converter) then
            raise (ArgumentException $"Can not convert null to '{expectedType}'")
        let instanceType = converter.GetType()
        if (expectedType.IsAssignableFrom instanceType = false) then
            raise (ArgumentException $"Can not convert '{instanceType}' to '{expectedType}'")
        converter

    static member GetMethod(t : Type, name : string) =
        let result = t.GetMethod name
        if isNull (box result) then
            raise (MissingMethodException $"Method not found, method name: {name}, type: {t}")
        result
