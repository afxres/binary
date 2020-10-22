namespace Mikodev.Binary.Creators.Sequence

open Mikodev.Binary
open System

[<CompiledName("FSharpListConverter`1")>]
type internal ListConverter<'T>(converter : Converter<'T>) =
    inherit Converter<List<'T>>(0)

    let constant = converter.Length > 0

    member private __.ExceptConstant(length : int) : unit =
        raise (ArgumentException(sprintf "Not enough bytes for collection element, byte length: %d, element type: %O" length typeof<'T>))

    member private me.DecodeConstant(span : ReadOnlySpan<byte>) : List<'T> =
        let converter = converter
        let itemLength = converter.Length
        let spanLength = span.Length;
        let quotient, remainder = Math.DivRem(spanLength, itemLength)
        if remainder <> 0 then
            me.ExceptConstant spanLength
        let mutable list = []
        let mutable i = quotient - 1
        while i >= 0 do
            let data = span.Slice(i * itemLength, itemLength)
            let head = converter.Decode &data
            list <- head :: list
            i <- i - 1
        list

    member private __.SelectVariable(span : byref<ReadOnlySpan<byte>>) : List<'T> =
        let converter = converter
        let data = ResizeArray<'T>()
        while not span.IsEmpty do
            data.Add(converter.DecodeAuto &span)
        let mutable list = []
        let mutable i = data.Count - 1
        while i >= 0 do
            let head = data.[i]
            list <- head :: list
            i <- i - 1
        list

    member private me.DecodeVariable(span : byref<ReadOnlySpan<byte>>, loop : int) : List<'T> =
        if span.IsEmpty then
            []
        elif loop < 64 then
            let head = converter.DecodeAuto &span
            let tail = me.DecodeVariable(&span, loop + 1)
            head :: tail
        else
            me.SelectVariable &span

    override __.Encode(allocator, item) =
        if isNull (box item) = false then
            let converter = converter
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : List<'T> =
        if constant then
            me.DecodeConstant span
        else
            let mutable body = span
            me.DecodeVariable(&body, 0)
