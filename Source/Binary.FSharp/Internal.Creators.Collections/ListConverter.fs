namespace Mikodev.Binary.Internal.Creators.Collections

open Mikodev.Binary
open System

[<CompiledName("FSharpListConverter`1")>]
type ListConverter<'T>(converter : Converter<Memory<'T>>) =
    inherit Converter<List<'T>>(0)

    override __.Encode(allocator, item) =
        if not (obj.ReferenceEquals(item, null)) then
            let memory = item |> List.toArray |> Memory
            converter.Encode(&allocator, memory)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : List<'T> =
        let data = (converter.Decode &span).Span
        let mutable list = []
        for i = data.Length - 1 downto 0 do
            list <- data.[i] :: list
        list
