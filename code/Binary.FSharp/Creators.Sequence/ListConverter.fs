namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open System
open System.Diagnostics

[<CompiledName("FSharpListConverter`1")>]
type internal ListConverter<'T>(converter : Converter<'T>) =
    inherit Converter<'T list>(0)

    [<Literal>]
    let capacity = 4

    let constant = converter.Length > 0

    [<DebuggerStepThrough>]
    member private __.ExceptConstant(length : int) : unit =
        raise (ArgumentException $"Not enough bytes for collection element, byte length: {length}, element type: {typeof<'T>}")

    member private me.DecodeConstant(span : ReadOnlySpan<byte>) : 'T list =
        let converter = converter
        let itemLength = converter.Length
        let spanLength = span.Length;
        if (spanLength % itemLength) <> 0 then
            me.ExceptConstant spanLength
        let mutable list = []
        let mutable i = spanLength - itemLength
        while i >= 0 do
            let data = span.Slice(i, itemLength)
            let head = converter.Decode &data
            list <- head :: list
            i <- i - itemLength
        list

    member private __.SelectVariable(span : byref<ReadOnlySpan<byte>>) : 'T list =
        let converter = converter
        let data = ResizeArray<'T> capacity
        while not span.IsEmpty do
            data.Add(converter.DecodeAuto &span)
        let mutable list = []
        let mutable i = data.Count - 1
        while i >= 0 do
            let head = data.[i]
            list <- head :: list
            i <- i - 1
        list

    member private me.DecodeVariable(span : byref<ReadOnlySpan<byte>>, loop : int) : 'T list =
        if span.IsEmpty then
            []
        elif loop > 0 then
            let head = converter.DecodeAuto &span
            let tail = me.DecodeVariable(&span, loop - 1)
            head :: tail
        else
            me.SelectVariable &span

    override __.Encode(allocator, item) =
        if isNull (box item) = false && List.isEmpty item = false then
            let converter = converter
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : 'T list =
        if span.IsEmpty then
            []
        elif constant then
            me.DecodeConstant span
        else
            let mutable body = span
            me.DecodeVariable(&body, capacity)
