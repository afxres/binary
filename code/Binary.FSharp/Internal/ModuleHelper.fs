module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Reflection.Emit
open System.Runtime.CompilerServices

type AllocatorToHandleDefinition = delegate of byref<Allocator> -> IntPtr

let EncodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Encode)

let DecodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Decode)

let AllocatorByRefType = (EncodeNumberMethodInfo.GetParameters() |> Array.head).ParameterType

let ReadOnlySpanByteByRefType = (DecodeNumberMethodInfo.GetParameters() |> Array.head).ParameterType

let AllocatorToHandleDelegate =
    let a = [| AllocatorByRefType |]
    let b = DynamicMethod("AllocatorToHandle", typeof<IntPtr>, a)
    let g = b.GetILGenerator()
    g.Emit OpCodes.Ldarg_0
    g.Emit OpCodes.Ret
    let f = b.CreateDelegate typeof<AllocatorToHandleDefinition>
    f :?> AllocatorToHandleDefinition

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let HandleToAllocator (data : IntPtr) = (# "" data : byref<Allocator> #)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let AllocatorToHandle (data : byref<Allocator>) = AllocatorToHandleDelegate.Invoke &data
