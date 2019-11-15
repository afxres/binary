namespace Mikodev.Binary.Creators

open Mikodev.Binary
open System
open System.Diagnostics
open System.Runtime.CompilerServices

type internal OfUnion<'a> = delegate of allocator : byref<Allocator> * item : 'a * mark : byref<int> -> unit

type internal ToUnion<'a> = delegate of span : byref<ReadOnlySpan<byte>> * mark : byref<int> -> 'a

type internal UnionConverter<'a>(encode : OfUnion<'a>, decode : ToUnion<'a>, encodeAuto : OfUnion<'a>, decodeAuto : ToUnion<'a>, noNull : bool) =
    inherit Converter<'a>(0)

    [<Literal>]
    let MarkNone = 0

    [<DebuggerStepThrough; MethodImpl(MethodImplOptions.NoInlining)>]
    member private me.ThrowNull () = raise (ArgumentNullException("item", sprintf "Union can not be null, type: %O" me.ItemType))

    [<DebuggerStepThrough; MethodImpl(MethodImplOptions.NoInlining)>]
    member private me.ThrowInvalid mark = raise (ArgumentException(sprintf "Invalid union tag '%d', type: %O" mark me.ItemType))

    member inline private me.ThrowOnNull item = if noNull && obj.ReferenceEquals(item, null) then me.ThrowNull()

    member inline private me.ThrowOnInvalid mark = if mark <> MarkNone then me.ThrowInvalid mark

    override me.Encode(allocator, item) =
        me.ThrowOnNull item
        let mutable mark = MarkNone
        encode.Invoke(&allocator, item, &mark)
        me.ThrowOnInvalid mark
        ()

    override me.Decode(span : inref<ReadOnlySpan<byte>>) : 'a =
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
