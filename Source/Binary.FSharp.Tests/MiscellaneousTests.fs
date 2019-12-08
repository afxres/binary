module MiscellaneousTests

open Microsoft.FSharp.Linq.RuntimeHelpers
open Mikodev.Binary
open Mikodev.Binary.Internal.Creators.Collections
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

[<Fact>]
let ``Collection Converter Or Creator Name`` () =
    let test (c : Type) (creator : Type) =
        let itemType = c.BaseType.GetGenericArguments() |> Array.exactlyOne
        let itemTypeName = itemType.Name
        Assert.StartsWith("FSharp", itemTypeName)

        let index = itemTypeName.IndexOf '`'
        let expectedConverterTypeName = itemTypeName.Insert(index, "Converter")
        let expectedConverterCreatorTypeName = itemTypeName.Substring(0, index) + "ConverterCreator"
        Assert.Equal(expectedConverterTypeName, c.Name)
        Assert.Equal(expectedConverterCreatorTypeName, creator.Name)
        ()

    test (typedefof<ListConverter<_>>) (typeof<ListConverterCreator>)
    test (typedefof<MapConverter<_, _>>) (typeof<MapConverterCreator>)
    test (typedefof<SetConverter<_>>) (typeof<SetConverterCreator>)
    ()
