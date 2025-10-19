namespace Mikodev.Binary.Creators

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

#nowarn "3261" // Nullness warning

[<CompiledName("FSharpMapConverter`2")>]
type internal MapConverter<'K, 'V when 'K: comparison>(init: Converter<'K>, tail: Converter<'V>) =
    inherit Converter<Map<'K, 'V>>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false && Map.isEmpty item = false then
            let init = init
            let tail = tail
            let handle = ModuleHelper.AllocatorToHandle &allocator
            item
            |> Map.iter (fun k v ->
                let allocator = &(ModuleHelper.HandleToAllocator handle)
                init.EncodeAuto(&allocator, k)
                tail.EncodeAuto(&allocator, v))
        ()

    override __.Decode(span: inref<ReadOnlySpan<byte>>) : Map<'K, 'V> =
        let mutable body = span
        let mutable map = Map.empty
        let init = init
        let tail = tail
        while not body.IsEmpty do
            let head = init.DecodeAuto &body
            let next = tail.DecodeAuto &body
            map <- Map.add head next map
        map
