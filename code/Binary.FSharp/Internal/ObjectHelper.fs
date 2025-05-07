module internal Mikodev.Binary.Internal.ObjectHelper

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Diagnostics
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

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
    let mutable cursor = span.Length - 1
    while cursor >= 0 do
        result <- span[cursor] :: result
        cursor <- cursor - 1
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
        box (GetListFromSpan source)
    else
        let result = ResizeArray<'E> NewLength
        result.AddRange source
        box result

let GetListWithConstantConverter<'E> (converter: Converter<'E>, span: ReadOnlySpan<byte>) =
    let itemLength = converter.Length
    let spanLength = span.Length
    if (spanLength % itemLength) <> 0 then
        ExceptConstant(spanLength, typeof<'E>)
    let mutable result = []
    let mutable cursor = spanLength - itemLength
    while cursor >= 0 do
        let data = span.Slice(cursor, itemLength)
        let head = converter.Decode &data
        result <- head :: result
        cursor <- cursor - itemLength
    result

let GetListWithVariableConverter<'E> (converter: Converter<'E>, span: ReadOnlySpan<byte>) =
    let mutable body = span
    match GetPartialList(converter, &body) with
    | :? list<'E> as result -> result
    | _ as intent ->
        let source = unbox<ResizeArray<'E>> intent
        assert (source.Count = MaxLevels)
        assert (source.Capacity = NewLength)
        while body.Length <> 0 do
            source.Add(converter.DecodeAuto(&body))
        GetListFromSpan(CollectionsMarshal.AsSpan source)

let GetList (converter: Converter<'E>, span: ReadOnlySpan<byte>) =
    if span.Length = 0 then []
    elif converter.Length <> 0 then GetListWithConstantConverter(converter, span)
    else GetListWithVariableConverter(converter, span)
