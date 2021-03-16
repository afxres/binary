module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Reflection
open System.Reflection.Emit

type private DefineAllocatorToHandle = delegate of byref<Allocator> -> IntPtr

type private Define() =
    static member Allocator(_ : byref<Allocator>) = ()

    static member ReadOnlySpan(_ : byref<ReadOnlySpan<byte>>) = ()

let private GetParameterType name =
    let m = typeof<Define>.GetMethod(name, BindingFlags.Static ||| BindingFlags.NonPublic)
    let p = m.GetParameters() |> Array.exactlyOne
    p.ParameterType

let private AllocatorToHandleDelegate =
    let a = [| GetParameterType (nameof Define.Allocator) |]
    let b = DynamicMethod("AllocatorToHandle", typeof<IntPtr>, a)
    let g = b.GetILGenerator()
    g.Emit OpCodes.Ldarg_0
    g.Emit OpCodes.Ret
    let f = b.CreateDelegate typeof<DefineAllocatorToHandle>
    f :?> DefineAllocatorToHandle

let AllocatorByRefType = GetParameterType (nameof Define.Allocator)

let ReadOnlySpanByteByRefType = GetParameterType (nameof Define.ReadOnlySpan)

let EncodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Encode, [| AllocatorByRefType; typeof<int> |])

let DecodeNumberMethodInfo = typeof<Converter>.GetMethod(nameof Converter.Decode, [| ReadOnlySpanByteByRefType |])

#nowarn "42" // This construct is deprecated: it is only for use in the F# library

let inline HandleToAllocator (data : IntPtr) = (# "" data : byref<Allocator> #)

let inline AllocatorToHandle (data : byref<Allocator>) = AllocatorToHandleDelegate.Invoke &data
