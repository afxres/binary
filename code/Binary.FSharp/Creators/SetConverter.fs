namespace Mikodev.Binary.Creators

open Mikodev.Binary
open Mikodev.Binary.Internal
open System

[<CompiledName("FSharpSetConverter`1")>]
type internal SetConverter<'T when 'T: comparison>(converter: Converter<'T>) =
    inherit Converter<Set<'T>>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false && Set.isEmpty item = false then
            let converter = converter
            let handle = ModuleHelper.AllocatorToHandle &allocator
            item
            |> Set.iter (fun x ->
                let allocator = &(ModuleHelper.HandleToAllocator handle)
                converter.EncodeAuto(&allocator, x))
        ()

    override __.Decode(span: inref<ReadOnlySpan<byte>>) : Set<'T> =
        let mutable body = span
        let mutable set = Set.empty
        let converter = converter
        while not body.IsEmpty do
            let i = converter.DecodeAuto &body
            set <- Set.add i set
        set
