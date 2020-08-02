module Miscellaneous.MiscellaneousTests

open Microsoft.FSharp.Linq.RuntimeHelpers
open Mikodev.Binary
open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Reflection
open Xunit

[<Fact>]
let ``Public Types`` () =
    let expression = <@ Func<_, _>(GeneratorBuilderFSharpExtensions.AddFSharpConverterCreators) @>
    let x = expression |> LeafExpressionConverter.QuotationToExpression
    let method = ((x :?> LambdaExpression).Body :?> MethodCallExpression).Method
    let types = method.DeclaringType.Assembly.GetTypes()
    let myTypes = types |> Array.filter (fun x -> x.IsPublic)
    let myTypeNames = myTypes |> Array.map (fun x -> x.Name)
    let names = [| "UnionEncoder`1"; "UnionDecoder`1"; "GeneratorBuilderFSharpExtensions" |] |> HashSet
    Assert.Equal(3, myTypeNames.Length)
    Assert.Equal<string>(names, myTypeNames |> HashSet)
    ()

[<Fact>]
let ``Public Members`` () =
    let expression = <@ Func<_, _>(GeneratorBuilderFSharpExtensions.AddFSharpConverterCreators) @>
    let x = expression |> LeafExpressionConverter.QuotationToExpression
    let method = ((x :?> LambdaExpression).Body :?> MethodCallExpression).Method
    let types = method.DeclaringType.Assembly.GetTypes()
    let myNonPublicTypes = types |> Array.filter (fun x -> x.Namespace.StartsWith "Mikodev.Binary.Creators" && not x.IsPublic)
    Assert.Equal(8, myNonPublicTypes.Length)
    for t in myNonPublicTypes do
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
