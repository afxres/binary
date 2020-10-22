module internal Mikodev.Binary.Internal.ModuleHelper

open Mikodev.Binary
open System
open System.Reflection
open System.Reflection.Emit
open System.Runtime.CompilerServices

[<Literal>]
let UnsafeHandleAssemblyName = "Mikodev.Binary.FSharp.Unsafe.Handle"

[<assembly : InternalsVisibleTo(UnsafeHandleAssemblyName)>]
do()

[<AbstractClass>]
type internal HandleHelper() =
    abstract AsHandle : allocator : byref<Allocator> -> IntPtr

    abstract AsAllocator : handle : IntPtr -> byref<Allocator>

let Handle : HandleHelper =
    let Make (t : TypeBuilder) (f : MethodInfo) =
        let b = t.DefineMethod(f.Name, MethodAttributes.Public ||| MethodAttributes.Virtual, f.ReturnType, f.GetParameters() |> Array.map (fun x -> x.ParameterType))
        let g = b.GetILGenerator()
        g.Emit OpCodes.Ldarg_1
        g.Emit OpCodes.Ret
        t.DefineMethodOverride(b, f)
        ()

    let a = AssemblyBuilder.DefineDynamicAssembly(AssemblyName UnsafeHandleAssemblyName, AssemblyBuilderAccess.Run)
    let m = a.DefineDynamicModule "InMemoryModule"
    let t = m.DefineType(typeof<HandleHelper>.Name, TypeAttributes.Public ||| TypeAttributes.Sealed, typeof<HandleHelper>)
    Make t (typeof<HandleHelper>.GetMethod "AsHandle")
    Make t (typeof<HandleHelper>.GetMethod "AsAllocator")
    let i = t.CreateType()
    let h = Activator.CreateInstance i
    h :?> HandleHelper
