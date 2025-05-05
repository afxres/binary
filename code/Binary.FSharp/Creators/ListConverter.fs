namespace Mikodev.Binary.Creators

open Mikodev.Binary
open System
open System.Diagnostics
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

module internal ListMethods =
    open System.Collections.Generic
    [<Literal>]
    let MaxLevels = 32

    [<Literal>]
    let NewLength = 64

    [<Struct>]
    [<NoEquality>]
    [<NoComparison>]
    [<InlineArray(MaxLevels)>]
    type InlineBuffer<'T> =
        [<DefaultValue(false)>]
        val mutable private Item: 'T

    [<DebuggerStepThrough>]
    let ExceptConstant (length: int, t: Type) : unit =
        raise (ArgumentException $"Not enough bytes for collection element, byte length: {length}, element type: {t}")

    let GetListFromSpan<'E> (span: Span<'E>) =
        let mutable result = []
        for i = span.Length - 1 downto 0 do
            result <- span[i] :: result
        result

    let GetPartialList<'E> (converter: Converter<'E>, span: byref<ReadOnlySpan<byte>>) =
        let mutable buffer = new InlineBuffer<'E>()
        let target = MemoryMarshal.CreateSpan(&Unsafe.As<InlineBuffer<'E>, 'E>(&buffer), MaxLevels)
        let mutable cursor = 0
        while cursor <> MaxLevels && span.Length <> 0 do
            target[cursor] <- converter.DecodeAuto &span
            cursor <- cursor + 1
        let source = target.Slice(0, cursor)
        if span.Length = 0 then
            Choice1Of2(GetListFromSpan source)
        else
            let result = ResizeArray<'E> NewLength
            CollectionExtensions.AddRange(result, source)
            Choice2Of2 result

    let GetListWithConstantConverter<'E> (converter: Converter<'E>, span: byref<ReadOnlySpan<byte>>) =
        let itemLength = converter.Length
        let spanLength = span.Length
        if (spanLength % itemLength) <> 0 then
            ExceptConstant(spanLength, typeof<'E>)
        let mutable result = []
        let mutable i = spanLength - itemLength
        while i >= 0 do
            let data = span.Slice(i, itemLength)
            let head = converter.Decode &data
            result <- head :: result
            i <- i - itemLength
        result

    let GetListWithVariableConverter<'E> (converter: Converter<'E>, span: byref<ReadOnlySpan<byte>>) =
        match GetPartialList(converter, &span) with
        | Choice1Of2 result -> result
        | Choice2Of2 source ->
            assert (source.Count = MaxLevels)
            assert (source.Capacity = NewLength)
            while span.Length <> 0 do
                source.Add(converter.DecodeAuto(&span))
            GetListFromSpan(CollectionsMarshal.AsSpan source)

    let GetList (converter: Converter<'E>, span: byref<ReadOnlySpan<byte>>) =
        if span.Length = 0 then []
        elif converter.Length <> 0 then GetListWithConstantConverter(converter, &span)
        else GetListWithVariableConverter(converter, &span)

[<CompiledName("FSharpListConverter`1")>]
type internal ListConverter<'T>(converter: Converter<'T>) =
    inherit Converter<'T list>(0)

    override __.Encode(allocator, item) =
        if isNull (box item) = false && List.isEmpty item = false then
            let converter = converter
            for i in item do
                converter.EncodeAuto(&allocator, i)
        ()

    override __.Decode(span: inref<ReadOnlySpan<byte>>) : 'T list =
        let mutable body = span
        ListMethods.GetList(converter, &body)
