module MiscellaneousTests

open Microsoft.FSharp.Linq.RuntimeHelpers
open Mikodev.Binary
open System
open System.Linq.Expressions
open Xunit

[<Fact>]
let ``Public Class And Method`` () =
    let expression = <@ Func<_, _>(GeneratorBuilderFSharpExtensions.AddFSharpConverterCreators) @>
    let x = expression |> LeafExpressionConverter.QuotationToExpression
    let method = ((x :?> LambdaExpression).Body :?> MethodCallExpression).Method
    let types = method.DeclaringType.Assembly.GetTypes()
    let myTypes = types |> Array.filter (fun x -> x.Namespace.StartsWith "Mikodev")
    Assert.NotEmpty myTypes
    Assert.All(myTypes, fun x -> Assert.True x.IsPublic)
    ()
