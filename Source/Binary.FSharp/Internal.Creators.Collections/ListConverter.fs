namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpListConverter`1")>]
type ListConverter<'T>(converter : Converter<'T>, memoryConverter : Converter<Memory<'T>>) =
    inherit Converter<List<'T>>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false then
            let converter = converter
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : List<'T> =
        let data = (memoryConverter.Decode &span).Span
        let mutable list = []
        for i = data.Length - 1 downto 0 do
            list <- data.[i] :: list
        list
