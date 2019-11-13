module TupleLike.TupleLikeTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``Generic Constraints`` () =
    let converterTypes = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.IsSubclassOf typeof<Converter> && x.Namespace.EndsWith "Tuples")
    Assert.Equal(17, converterTypes.Length)
    let arguments = converterTypes |> Array.map (fun x -> x.GetGenericArguments()) |> Array.concat
    let collection = arguments |> Array.map (fun x -> (x, x.GetGenericParameterConstraints())) |> Array.filter (fun (_, b) -> not (Array.isEmpty b))
    let argument, constraints = Assert.Single(collection)
    Assert.StartsWith("ValueTupleConverter`8", argument.DeclaringType.Name)
    Assert.Equal(typeof<ValueType>, Assert.Single(constraints))
    ()
