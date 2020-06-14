namespace Mikodev.Binary.Internal.Creators

open Mikodev.Binary
open System

type UnionEncoder<'T> = delegate of allocator : byref<Allocator> * item : 'T * mark : byref<int> -> unit

type UnionDecoder<'T> = delegate of span : byref<ReadOnlySpan<byte>> * mark : byref<int> -> 'T

type UnionConverter<'T>(encode : UnionEncoder<'T>, decode : UnionDecoder<'T>, encodeAuto : UnionEncoder<'T>, decodeAuto : UnionDecoder<'T>, noNull : bool) =
    inherit Converter<'T>(0)

    [<Literal>]
    let MarkNone = 0

    member private me.NotifyNull() : unit = raise (ArgumentNullException("item", sprintf "Union can not be null, type: %O" me.ItemType))

    member private me.NotifyMark(mark : int) : unit = raise (ArgumentException(sprintf "Invalid union tag '%d', type: %O" mark me.ItemType))

    member inline private me.DetectNull(item : 'T) : unit = if noNull && isNull (box item) then me.NotifyNull()

    member inline private me.DetectMark(mark : int) : unit = if mark <> MarkNone then me.NotifyMark mark

    override me.Encode(allocator, item) =
        me.DetectNull item
        let mutable mark = MarkNone
        encode.Invoke(&allocator, item, &mark)
        me.DetectMark mark
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : 'T =
        let mutable body = span
        let mutable mark = MarkNone
        let item = decode.Invoke(&body, &mark)
        me.DetectMark mark
        item

    override me.EncodeAuto(allocator, item) =
        me.DetectNull item
        let mutable mark = MarkNone
        encodeAuto.Invoke(&allocator, item, &mark)
        me.DetectMark mark
        ()

    override me.DecodeAuto span =
        let mutable mark = MarkNone
        let item = decodeAuto.Invoke(&span, &mark)
        me.DetectMark mark
        item
