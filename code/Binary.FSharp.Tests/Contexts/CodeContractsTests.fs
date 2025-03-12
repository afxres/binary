module Contexts.CodeContractsTests

open Mikodev.Binary
open System.Collections.Generic
open System.Diagnostics.CodeAnalysis
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
let ``Public Members With Requires Dynamic Code Attribute`` () =
    let t = typeof<GeneratorBuilderFSharpExtensions>
    let members = t.GetMembers(BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.Public)
    let common = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "CommonDefine") |> Array.exactlyOne
    let messageField = common.GetField("RequiresDynamicCodeMessage", BindingFlags.Static ||| BindingFlags.NonPublic)
    Assert.NotNull messageField
    let message = messageField.GetValue(null)
    Assert.NotNull message
    Assert.NotEmpty members
    let sequence = seq {
        for m in members do
            if m.DeclaringType <> typeof<obj> then
                let attribute =
                    m.GetCustomAttributes()
                    |> Seq.choose (fun x ->
                        match x with
                        | :? RequiresDynamicCodeAttribute as a -> Some a
                        | _ -> None)
                    |> Seq.exactlyOne
                yield attribute.Message
    }
    let actual = sequence |> Seq.exactlyOne
    Assert.Equal(message, actual)
    ()

[<Fact>]
let ``Public Members With Requires Unreferenced Code Attribute`` () =
    let t = typeof<GeneratorBuilderFSharpExtensions>
    let members = t.GetMembers(BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.Public)
    let common = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "CommonDefine") |> Array.exactlyOne
    let messageField = common.GetField("RequiresUnreferencedCodeMessage", BindingFlags.Static ||| BindingFlags.NonPublic)
    Assert.NotNull messageField
    let message = messageField.GetValue(null)
    Assert.NotNull message
    Assert.NotEmpty members
    let sequence = seq {
        for m in members do
            if m.DeclaringType <> typeof<obj> then
                let attribute =
                    m.GetCustomAttributes()
                    |> Seq.choose (fun x ->
                        match x with
                        | :? RequiresUnreferencedCodeAttribute as a -> Some a
                        | _ -> None)
                    |> Seq.exactlyOne
                yield attribute.Message
    }
    let actual = sequence |> Seq.exactlyOne
    Assert.Equal(message, actual)
    ()
