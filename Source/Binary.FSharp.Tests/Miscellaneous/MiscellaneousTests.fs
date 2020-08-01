module Miscellaneous.MiscellaneousTests

open Microsoft.FSharp.Linq.RuntimeHelpers
open Mikodev.Binary
open System
open System.Collections.Generic
open System.Linq.Expressions
open Xunit

[<Fact>]
let ``Public Class And Method`` () =
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
