namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open System

[<CompiledName("FSharpMapConverter`2")>]
type internal MapConverter<'K, 'V when 'K : comparison>(initConverter : Converter<'K>, tailConverter : Converter<'V>) =
    inherit Converter<Map<'K, 'V>>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false then
            let initConverter = initConverter
            let tailConverter = tailConverter
            let handle = AllocatorUnsafeHandle &allocator
            item |> Map.iter (fun k v ->
                let allocator = &handle.AsAllocator()
                initConverter.EncodeAuto(&allocator, k)
                tailConverter.EncodeAuto(&allocator, v))
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Map<'K, 'V> =
        let mutable body = span
        let mutable map = Map.empty
        let initConverter = initConverter
        let tailConverter = tailConverter
        while not body.IsEmpty do
            let init = initConverter.DecodeAuto &body
            let tail = tailConverter.DecodeAuto &body
            map <- Map.add init tail map
        map
