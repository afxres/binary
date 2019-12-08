namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System
open System.Collections.Generic

type MapConverter<'K, 'V when 'K : comparison>(converter : Converter<KeyValuePair<'K, 'V>>) =
    inherit Converter<Map<'K, 'V>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Map<'K, 'V> =
        let mutable span = span
        let mutable map = Map.empty
        while not span.IsEmpty do
            let pair = converter.DecodeAuto &span
            map <- Map.add pair.Key pair.Value map
        map
