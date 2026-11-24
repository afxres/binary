module Creators.UnionConverterTests

open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection
open Mikodev.Binary
open System
open System.Reflection
open System.Reflection.Emit
open Xunit

/// F# forbids applying CompilationMappingAttribute in source (FS0850),
/// so emit a type that only looks like a sum type and has no union case at all
let private CreateEmptyUnionType () =
    let assemblyName = AssemblyName("EmptyUnion")
    let assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
    let moduleBuilder = assemblyBuilder.DefineDynamicModule("EmptyUnion")
    let typeBuilder = moduleBuilder.DefineType("EmptyUnion", TypeAttributes.Public ||| TypeAttributes.Class)
    let constructor = typeof<CompilationMappingAttribute>.GetConstructor([| typeof<SourceConstructFlags> |])
    typeBuilder.SetCustomAttribute(CustomAttributeBuilder(constructor, [| box SourceConstructFlags.SumType |]))
    typeBuilder.CreateType()

/// FSharp.Core reads Assembly.ReflectionOnly, which the Mono runtime used by the
/// s390x build does not implement for dynamic assemblies, so an emitted type
/// cannot be inspected there at all (mono/mono#13028).
let private CanInspectEmittedType (t: Type) =
    try
        FSharpType.IsUnion(t) |> ignore
        true
    with
    | :? NotImplementedException
    | :? PlatformNotSupportedException -> false

[<Fact>]
let ``Empty Union Type`` () =
    let t = CreateEmptyUnionType()
    Assert.SkipWhen(not (CanInspectEmittedType t), "the runtime cannot reflect over emitted types")
    Assert.True(FSharpType.IsUnion(t))
    Assert.Empty(FSharpType.GetUnionCases(t))
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "No available union case found, type: %O" t
    Assert.Equal(message, error.Message)
    ()
