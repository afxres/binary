﻿module Contexts.GeneratorBuilderTests

open Mikodev.Binary
open System
open Xunit

let GeneratorBuilder () =
    let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorBuilder") |> Array.exactlyOne
    let builder = Activator.CreateInstance(t)
    builder :?> IGeneratorBuilder

type FakeConverterA<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type FakeConverterB<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type FakeConverterCreatorA() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = raise (NotSupportedException("Text alpha"))

type FakeConverterCreatorB() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = raise (NotSupportedException("Text bravo"))

[<Fact>]
let ``Build Empty`` () =
    let builder = GeneratorBuilder()
    let generator = builder.Build()
    Assert.Equal("Converter Count = 1, Converter Creator Count = 0", generator.ToString())
    ()

[<Fact>]
let ``Add Null Converter`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> GeneratorBuilder().AddConverter null |> ignore)
    let method = typeof<IGeneratorBuilder>.GetMethod("AddConverter")
    let parameter = method.GetParameters() |> Array.exactlyOne
    Assert.Equal("converter", parameter.Name)
    Assert.Equal("converter", error.ParamName)
    ()

[<Fact>]
let ``Add Null Converter Creator`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> GeneratorBuilder().AddConverterCreator null |> ignore)
    let method = typeof<IGeneratorBuilder>.GetMethod("AddConverterCreator")
    let parameter = method.GetParameters() |> Array.exactlyOne
    Assert.Equal("creator", parameter.Name)
    Assert.Equal("creator", error.ParamName)
    ()

[<Fact>]
let ``Add Two Converters For One Type`` () =
    let Test a b =
        let builder = GeneratorBuilder().AddConverter(a).AddConverter(b)
        let generator = builder.Build()
        Assert.Equal("Converter Count = 2, Converter Creator Count = 0", generator.ToString())
        let converter = generator.GetConverter<int>()
        Assert.True(obj.ReferenceEquals(b, converter))
        ()

    Test (FakeConverterA<int>()) (FakeConverterB<int>())
    Test (FakeConverterB<int>()) (FakeConverterA<int>())
    ()

[<Fact>]
let ``Last Added Creator First Executed`` () =
    let Test a b message =
        let builder = GeneratorBuilder().AddConverterCreator(a).AddConverterCreator(b)
        let generator = builder.Build()
        Assert.Equal("Converter Count = 1, Converter Creator Count = 2", generator.ToString())
        let error = Assert.Throws<NotSupportedException>(fun () -> generator.GetConverter<int>() |> ignore)
        Assert.Equal(message, error.Message)
        ()

    Test (FakeConverterCreatorA()) (FakeConverterCreatorB()) "Text bravo"
    Test (FakeConverterCreatorB()) (FakeConverterCreatorA()) "Text alpha"
    ()

[<Fact>]
let ``Invalid Converter For Object`` () =
    let builder = GeneratorBuilder()
    let error = Assert.Throws<ArgumentException>(fun () -> builder.AddConverter(FakeConverterA<obj>()) |> ignore)
    Assert.Equal(sprintf "Can not add converter for '%O'" typeof<obj>, error.Message)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let builder = GeneratorBuilder()
    Assert.Equal("Converter Count = 0, Converter Creator Count = 0", builder.ToString())
    ()
