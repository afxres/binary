namespace Mikodev.Binary.Creators

open Mikodev.Binary
open System
open System.Diagnostics
open System.Runtime.CompilerServices

type internal UnionEncoder<'T> = delegate of allocator: byref<Allocator> * item: 'T * mark: byref<int> -> unit

type internal UnionDecoder<'T> = delegate of span: byref<ReadOnlySpan<byte>> * mark: byref<int> -> 'T

type internal UnionConverter<'T>() =
    inherit Converter<'T>(0)

    [<Literal>]
    let constant = 0

    [<DefaultValue>]
    val mutable encode: UnionEncoder<'T>

    [<DefaultValue>]
    val mutable encodeAuto: UnionEncoder<'T>

    [<DefaultValue>]
    val mutable decode: UnionDecoder<'T>

    [<DefaultValue>]
    val mutable decodeAuto: UnionDecoder<'T>

    [<DefaultValue>]
    val mutable needNullCheck: bool

    [<DebuggerStepThrough>]
    member private __.ExceptNull() : unit =
        raise (ArgumentNullException("item", $"Union can not be null, type: {typeof<'T>}"))

    [<DebuggerStepThrough>]
    member private __.ExceptMark(mark: int) : unit = raise (ArgumentException $"Invalid union tag '{mark}', type: {typeof<'T>}")

    [<DebuggerStepThrough>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.HandleNull(item: 'T) : unit =
        if me.needNullCheck && isNull (box item) then
            me.ExceptNull()
        ()

    [<DebuggerStepThrough>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private me.HandleMark(mark: int) : unit =
        if mark <> constant then
            me.ExceptMark mark
        ()

    member me.Initialize(encode: UnionEncoder<'T>, encodeAuto: UnionEncoder<'T>, decode: UnionDecoder<'T>, decodeAuto: UnionDecoder<'T>, needNullCheck: bool) =
        assert (isNull me.encode)
        assert (isNull me.encodeAuto)
        me.encode <- encode
        me.encodeAuto <- encodeAuto
        me.decode <- decode
        me.decodeAuto <- decodeAuto
        me.needNullCheck <- needNullCheck
        ()

    override me.Encode(allocator, item) =
        me.HandleNull item
        let mutable mark = constant
        me.encode.Invoke(&allocator, item, &mark)
        me.HandleMark mark
        ()

    override me.EncodeAuto(allocator, item) =
        me.HandleNull item
        let mutable mark = constant
        me.encodeAuto.Invoke(&allocator, item, &mark)
        me.HandleMark mark
        ()

    override me.Decode(span: inref<ReadOnlySpan<byte>>) : 'T =
        let mutable body = span
        let mutable mark = constant
        let item = me.decode.Invoke(&body, &mark)
        me.HandleMark mark
        item

    override me.DecodeAuto span =
        let mutable mark = constant
        let item = me.decodeAuto.Invoke(&span, &mark)
        me.HandleMark mark
        item
