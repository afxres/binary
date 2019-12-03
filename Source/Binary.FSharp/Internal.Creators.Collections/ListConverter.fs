namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

type ListConverter<'a>(converter : Converter<'a> , memoryConverter : Converter<Memory<'a>>) =
    inherit Converter<List<'a>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : List<'a> =
        let data = (memoryConverter.Decode &span).Span
        let mutable list = []
        for i = data.Length - 1 downto 0 do
            list <- data.[i] :: list
        list
