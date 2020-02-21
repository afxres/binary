namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpMapConverter`2")>]
type MapConverter<'K, 'V when 'K : comparison>(headConverter : Converter<'K>, dataConverter : Converter<'V>) =
    inherit Converter<Map<'K, 'V>>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false then
            let headConverter = headConverter
            let dataConverter = dataConverter
            let handle = AllocatorUnsafeHandle &allocator
            item |> Map.iter (fun k v ->
                let allocator = &handle.AsAllocator()
                headConverter.EncodeAuto(&allocator, k)
                dataConverter.EncodeAuto(&allocator, v))
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Map<'K, 'V> =
        let mutable body = span
        let mutable map = Map.empty
        let headConverter = headConverter
        let dataConverter = dataConverter
        while not body.IsEmpty do
            let head = headConverter.DecodeAuto &body
            let data = dataConverter.DecodeAuto &body
            map <- Map.add head data map
        map
