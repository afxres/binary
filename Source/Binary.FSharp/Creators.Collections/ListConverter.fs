namespace Mikodev.Binary.Creators.Collections

open Mikodev.Binary
open System

type internal ListConverter<'a>(converter : Converter<'a> , arraySegmentConverter : Converter<ArraySegment<'a>>) =
    inherit Converter<List<'a>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : List<'a> =
        let source = arraySegmentConverter.Decode &span
        let buffer = source.Array
        let length = source.Count
        let offset = source.Offset
        let mutable list = []
        for i = (offset + length - 1) downto offset do
            list <- buffer.[i] :: list
        list
