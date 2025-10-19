namespace Mikodev.Binary.Creators

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

#nowarn "3261" // Nullness warning

[<CompiledName("FSharpListConverter`1")>]
type internal ListConverter<'T>(converter: Converter<'T>) =
    inherit Converter<'T list>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false && List.isEmpty item = false then
            let converter = converter
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span: inref<ReadOnlySpan<byte>>) : 'T list = ObjectHelper.GetList(converter, span)
