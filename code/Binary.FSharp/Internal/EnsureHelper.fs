module internal Mikodev.Binary.Internal.EnsureHelper

open Mikodev.Binary
open System

let EnsureConverter (context : IGeneratorContext) (t : Type) =
    let converter = context.GetConverter t
    let expectedType = typedefof<Converter<_>>.MakeGenericType t
    if isNull (box converter) then
        raise (ArgumentException(sprintf "Can not convert 'null' to '%O'" expectedType))
    let instanceType = converter.GetType()
    if not (expectedType.IsAssignableFrom instanceType) then
        raise (ArgumentException(sprintf "Can not convert '%O' to '%O'" instanceType expectedType))
    converter
