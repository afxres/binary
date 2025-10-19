module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Runtime.CompilerServices

let AllocatorByRefType = (CommonHelper.GetType(typeof<IConverter>.Assembly, "Mikodev.Binary.Allocator") |> nonNull).MakeByRefType()

let ReadOnlySpanByteByRefType = CommonHelper.GetMethod((AllocatorByRefType.GetElementType() |> nonNull), "AsSpan", Array.empty).ReturnType.MakeByRefType()

let EncodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, "Encode", [| AllocatorByRefType; typeof<int> |])

let DecodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, "Decode", [| ReadOnlySpanByteByRefType |])

let EnsureSufficientExecutionStackMethodInfo = CommonHelper.GetMethod(typeof<RuntimeHelpers>, "EnsureSufficientExecutionStack", Array.empty)

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let HandleToAllocator (data: nativeint) = (# "" data : byref<Allocator> #)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let AllocatorToHandle (data: byref<Allocator>) = (# "conv.u" &data : nativeint #)
