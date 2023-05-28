module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Runtime.CompilerServices
open System.Reflection

type IdentityDefinition = delegate of nativeint -> nativeint

type ToHandleDefinition = delegate of byref<Allocator> -> nativeint

let IdentityDelegate = IdentityDefinition id

let AllocatorByRefType = typeof<IConverter>.Assembly.GetType("Mikodev.Binary.Allocator", throwOnError = true).MakeByRefType()

let ReadOnlySpanByteByRefType = typeof<ReadOnlyMemory<byte>>.GetProperty("Span", BindingFlags.Instance ||| BindingFlags.Public).PropertyType.MakeByRefType()

let EncodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, "Encode", [| AllocatorByRefType; typeof<int> |])

let DecodeNumberMethodInfo = CommonHelper.GetMethod(typeof<Converter>, "Decode", [| ReadOnlySpanByteByRefType |])

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let HandleToAllocator (data : nativeint) = (# "" data : byref<Allocator> #)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let AllocatorToHandle (data : byref<Allocator>) = Unsafe.As<ToHandleDefinition>(IdentityDelegate).Invoke &data
