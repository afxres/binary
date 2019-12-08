namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpSetConverter`1")>]
type SetConverter<'T when 'T : comparison>(converter : Converter<'T>) =
    inherit Converter<Set<'T>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Set<'T> =
        let mutable span = span
        let mutable set = Set.empty
        while not span.IsEmpty do
            let i = converter.DecodeAuto &span
            set <- Set.add i set
        set
