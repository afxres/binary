namespace Mikodev.Binary.Creators.Collections

open Mikodev.Binary
open System

type internal ListConverter<'a>(converter : Converter<'a> , memoryConverter : Converter<Memory<'a>>) =
    inherit Converter<List<'a>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : List<'a> =
        let memory = memoryConverter.Decode &span
        let span = memory.Span
        let mutable list = []
        for i = span.Length - 1 downto 0 do
            list <- span.[i] :: list
        list
