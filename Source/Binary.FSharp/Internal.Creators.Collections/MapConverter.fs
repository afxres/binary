namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System
open System.Collections.Generic

type MapConverter<'a, 'b when 'a : comparison>(converter : Converter<KeyValuePair<'a, 'b>>) =
    inherit Converter<Map<'a, 'b>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Map<'a, 'b> =
        let mutable span = span
        let mutable map = Map.empty
        while not span.IsEmpty do
            let pair = converter.DecodeAuto &span
            map <- Map.add pair.Key pair.Value map
        map
