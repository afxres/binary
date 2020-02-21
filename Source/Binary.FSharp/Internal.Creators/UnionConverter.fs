namespace Mikodev.Binary.Internal.Creators

open Mikodev.Binary
open System
open System.Diagnostics
open System.Runtime.CompilerServices

type UnionEncoder<'T> = delegate of allocator : byref<Allocator> * item : 'T * mark : byref<int> -> unit

type UnionDecoder<'T> = delegate of span : byref<ReadOnlySpan<byte>> * mark : byref<int> -> 'T

type UnionConverter<'T>(encode : UnionEncoder<'T>, decode : UnionDecoder<'T>, encodeAuto : UnionEncoder<'T>, decodeAuto : UnionDecoder<'T>, noNull : bool) =
    inherit Converter<'T>(0)

    [<Literal>]
    let MarkNone = 0

    [<DebuggerStepThrough; MethodImpl(MethodImplOptions.NoInlining)>]
    member private me.ThrowNull () = raise (ArgumentNullException("item", sprintf "Union can not be null, type: %O" me.ItemType))

    [<DebuggerStepThrough; MethodImpl(MethodImplOptions.NoInlining)>]
    member private me.ThrowInvalid mark = raise (ArgumentException(sprintf "Invalid union tag '%d', type: %O" mark me.ItemType))

    member inline private me.ThrowOnNull item = if noNull && isNull (box item) then me.ThrowNull()

    member inline private me.ThrowOnInvalid mark = if mark <> MarkNone then me.ThrowInvalid mark

    override me.Encode(allocator, item) =
        me.ThrowOnNull item
        let mutable mark = MarkNone
        encode.Invoke(&allocator, item, &mark)
        me.ThrowOnInvalid mark
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : 'T =
        let mutable span = span
        let mutable mark = MarkNone
        let item = decode.Invoke(&span, &mark)
        me.ThrowOnInvalid mark
        item

    override me.EncodeAuto(allocator, item) =
        me.ThrowOnNull item
        let mutable mark = MarkNone
        encodeAuto.Invoke(&allocator, item, &mark)
        me.ThrowOnInvalid mark
        ()

    override me.DecodeAuto span =
        let mutable mark = MarkNone
        let item = decodeAuto.Invoke(&span, &mark)
        me.ThrowOnInvalid mark
        item
