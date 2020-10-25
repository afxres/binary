module Contexts.CodeContractsTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

[<Fact>]
let ``Public Types`` () =
    let types = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes()
    let myTypes = types |> Array.filter (fun x -> x.IsPublic)
    let myTypeNames = myTypes |> Array.map (fun x -> x.Name)
    let names = [| "GeneratorBuilderFSharpExtensions" |] |> HashSet
    Assert.Equal(1, myTypeNames.Length)
    Assert.Equal<string>(names, myTypeNames |> HashSet)
    ()

[<Fact>]
let ``Public Members`` () =
    let types = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes()
    let myNonPublicTypes = types |> Array.filter (fun x -> x.Namespace.StartsWith "Mikodev.Binary.Creators" && not x.IsPublic)
    Assert.Equal(10, myNonPublicTypes.Length)
    let myNonPublicNonDelegateTypes = myNonPublicTypes |> Array.filter (fun x -> not (x.IsSubclassOf typeof<Delegate>))
    Assert.Equal(8, myNonPublicNonDelegateTypes.Length)
    for t in myNonPublicNonDelegateTypes do
        let members = t.GetMembers()
        let constructor = members |> Array.choose (fun x -> match x with | :? ConstructorInfo as info -> Some info | _ -> None) |> Array.exactlyOne
        let otherMembers = members |> Array.except [| constructor :> MemberInfo |]
        let virtualMembers = otherMembers |> Array.choose (fun x -> match x with | :? MethodBase as info -> (if info.IsVirtual then Some (info :> MemberInfo) else None) | _ -> None)
        let nonVirtualMembers = otherMembers |> Array.except virtualMembers
        let declaringTypes = nonVirtualMembers |> Array.map (fun x -> x.DeclaringType) |> Array.distinct
        let declaringTypeNames = declaringTypes |> Array.map (fun x -> x.Name)
        let expectedTypeNames = [| "Object"; "Converter`1" |]
        Assert.All(declaringTypeNames, fun x -> Assert.Contains(x, expectedTypeNames))
    ()
