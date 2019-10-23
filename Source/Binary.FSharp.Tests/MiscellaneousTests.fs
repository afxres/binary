module MiscellaneousTests

open Microsoft.FSharp.Linq.RuntimeHelpers
open Mikodev.Binary
open System
open System.Linq.Expressions
open System.Reflection
open Xunit

[<Fact>]
let ``Public Class And Method`` () =
    let expression = <@ Func<_, _>(GeneratorBuilderFSharpExtensions.AddFSharpConverterCreators) @>
    let x = expression |> LeafExpressionConverter.QuotationToExpression
    let method = ((x :?> LambdaExpression).Body :?> MethodCallExpression).Method
    let types = method.DeclaringType.Assembly.GetTypes()
    let publicTypes = types |> Array.filter (fun x -> x.IsPublic)
    let publicType = Assert.Single(publicTypes)
    let publicMembers = publicType.GetMembers() |> Array.filter (fun x -> x.DeclaringType = publicType)
    let publicMember = Assert.IsAssignableFrom<MethodInfo>(Assert.Single(publicMembers))
    Assert.Equal("AddFSharpConverterCreators", publicMember.Name)
    ()
