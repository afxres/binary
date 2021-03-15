module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Reflection
open System.Runtime.InteropServices

type private DefineAllocatorToHandle = delegate of byref<Allocator> -> IntPtr

type private Define() =
    static member Allocator(_ : byref<Allocator>) = raise null

    static member ReadOnlySpan(_ : byref<ReadOnlySpan<byte>>) = raise null

    static member Identity(data : IntPtr) = data

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

let private GetMethod name = typeof<Define>.GetMethod(name, BindingFlags.Static ||| BindingFlags.NonPublic)

let private GetParameterType name = let m = GetMethod name in let p = m.GetParameters() |> Array.exactlyOne in p.ParameterType

let private Identity = GetMethod (nameof Define.Identity)

let private IdentityDelegate<'T> () = Marshal.GetDelegateForFunctionPointer<'T>(Identity.MethodHandle.GetFunctionPointer())

let private AllocatorToHandleDelegate = IdentityDelegate<DefineAllocatorToHandle> ()

let AllocatorByRefType = GetParameterType (nameof Define.Allocator)

let ReadOnlySpanByteByRefType = GetParameterType (nameof Define.ReadOnlySpan)

let EncodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Encode, [| AllocatorByRefType; typeof<int> |])

let DecodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Decode, [| ReadOnlySpanByteByRefType |])

let inline HandleToAllocator (data : IntPtr) = (# "" data : byref<Allocator> #)

let inline AllocatorToHandle (data : byref<Allocator>) = AllocatorToHandleDelegate.Invoke &data
