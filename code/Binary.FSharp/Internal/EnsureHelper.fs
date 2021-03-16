module internal Mikodev.Binary.Internal.EnsureHelper

open Mikodev.Binary
open System

let EnsureConverter (context : IGeneratorContext) (t : Type) =
    let converter = context.GetConverter t
    let expectedType = MakeGenericType<Converter<_>> t
    if isNull (box converter) then
        raise (ArgumentException $"Can not convert null to '{expectedType}'")
    let instanceType = converter.GetType()
    if not (expectedType.IsAssignableFrom instanceType) then
        raise (ArgumentException $"Can not convert '{instanceType}' to '{expectedType}'")
    converter
