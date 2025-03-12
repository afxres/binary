namespace Mikodev.Binary.Creators

open Mikodev.Binary
open System
open System.Diagnostics
open System.Runtime.CompilerServices

type internal UnionEncoder<'T> = delegate of allocator: byref<Allocator> * item: 'T * mark: byref<int> -> unit

type internal UnionDecoder<'T> = delegate of span: byref<ReadOnlySpan<byte>> * mark: byref<int> -> 'T

type internal UnionConverter<'T>(encode: UnionEncoder<'T>, encodeAuto: UnionEncoder<'T>, decode: UnionDecoder<'T>, decodeAuto: UnionDecoder<'T>, noNull: bool) =
    inherit Converter<'T>(0)

    [<Literal>]
    let constant = 0

    [<DebuggerStepThrough>]
    member private __.ExceptNull() : unit =
        raise (ArgumentNullException("item", $"Union can not be null, type: {typeof<'T>}"))

    [<DebuggerStepThrough>]
    member private __.ExceptMark(mark: int) : unit = raise (ArgumentException $"Invalid union tag '{mark}', type: {typeof<'T>}")

    [<DebuggerStepThrough>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.HandleNull(item: 'T) : unit =
        if noNull && isNull (box item) then
            me.ExceptNull()
        ()

    [<DebuggerStepThrough>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.HandleMark(mark: int) : unit =
        if mark <> constant then
            me.ExceptMark mark
        ()

    override me.Encode(allocator, item) =
        me.HandleNull item
        let mutable mark = constant
        encode.Invoke(&allocator, item, &mark)
        me.HandleMark mark
        ()

    override me.EncodeAuto(allocator, item) =
        me.HandleNull item
        let mutable mark = constant
        encodeAuto.Invoke(&allocator, item, &mark)
        me.HandleMark mark
        ()

    override me.Decode(span: inref<ReadOnlySpan<byte>>) : 'T =
        let mutable body = span
        let mutable mark = constant
        let item = decode.Invoke(&body, &mark)
        me.HandleMark mark
        item

    override me.DecodeAuto span =
        let mutable mark = constant
        let item = decodeAuto.Invoke(&span, &mark)
        me.HandleMark mark
        item
