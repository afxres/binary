namespace Mikodev.Binary.Creators.Fallback

open Mikodev.Binary
open Mikodev.Binary.Internal
open System
open System.Runtime.CompilerServices

type internal UnionConverter<'T>(encode : UnionEncoder<'T>, encodeAuto : UnionEncoder<'T>, decode : UnionDecoder<'T>, decodeAuto : UnionDecoder<'T>, noNull : bool) =
    inherit Converter<'T>(0)

    [<Literal>]
    let MarkNone = 0

    member private __.NotifyNull() : unit =
        raise (ArgumentNullException("item", sprintf "Union can not be null, type: %O" typeof<'T>))

    member private __.NotifyMark(mark : int) : unit =
        raise (ArgumentException(sprintf "Invalid union tag '%d', type: %O" mark typeof<'T>))

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.DetectNull(item : 'T) : unit =
        if noNull && isNull (box item) then
            me.NotifyNull()
        ()

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.DetectMark(mark : int) : unit =
        if mark <> MarkNone then
            me.NotifyMark mark
        ()

    override me.Encode(allocator, item) =
        me.DetectNull item
        let mutable mark = MarkNone
        encode.Invoke(&allocator, item, &mark)
        me.DetectMark mark
        ()

    override me.EncodeAuto(allocator, item) =
        me.DetectNull item
        let mutable mark = MarkNone
        encodeAuto.Invoke(&allocator, item, &mark)
        me.DetectMark mark
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : 'T =
        let mutable body = span
        let mutable mark = MarkNone
        let item = decode.Invoke(&body, &mark)
        me.DetectMark mark
        item

    override me.DecodeAuto span =
        let mutable mark = MarkNone
        let item = decodeAuto.Invoke(&span, &mark)
        me.DetectMark mark
        item
