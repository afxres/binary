module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System.Runtime.CompilerServices

type IdentityDefinition = delegate of nativeint -> nativeint

type ToHandleDefinition = delegate of byref<Allocator> -> nativeint

let IdentityDelegate = IdentityDefinition id

let EncodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, nameof Converter.Encode)

let DecodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, nameof Converter.Decode)

let AllocatorByRefType = (EncodeNumberMethodInfo.GetParameters() |> Array.head).ParameterType

let ReadOnlySpanByteByRefType = (DecodeNumberMethodInfo.GetParameters() |> Array.head).ParameterType

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let HandleToAllocator (data : nativeint) = (# "" data : byref<Allocator> #)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let AllocatorToHandle (data : byref<Allocator>) = Unsafe.As<ToHandleDefinition>(IdentityDelegate).Invoke &data
