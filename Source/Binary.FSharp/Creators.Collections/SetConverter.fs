namespace Mikodev.Binary.Creators.Collections

open Mikodev.Binary
open System

type internal SetConverter<'a when 'a : comparison>(converter : Converter<'a>) =
    inherit Converter<Set<'a>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Set<'a> =
        let mutable span = span
        let mutable set = Set.empty
        while not span.IsEmpty do
            let i = converter.DecodeAuto &span
            set <- Set.add i set
        set
