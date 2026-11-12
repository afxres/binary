module Creators.NullableConverterTests

open Mikodev.Binary
open System
open Xunit

let private generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()

let private Encode (converter: IConverter) (value: obj) =
    let mutable allocator = Allocator()
    converter.Encode(&allocator, value)
    allocator.AsSpan().ToArray()

let private EncodeAuto (converter: IConverter) (value: obj) =
    let mutable allocator = Allocator()
    converter.EncodeAuto(&allocator, value)
    allocator.AsSpan().ToArray()

/// F# option and Nullable<'T> must produce identical wire bytes
let private Comparison (value: obj) (optionValue: obj) =
    let valueType = value.GetType()
    let itemType = typeof<Nullable<int>>.GetGenericTypeDefinition().MakeGenericType(valueType)
    let optionType = typeof<int option>.GetGenericTypeDefinition().MakeGenericType(valueType)
    let converterItem = generator.GetConverter(itemType)
    let converterOption = generator.GetConverter(optionType)
    Assert.StartsWith("NullableConverter`1", converterItem.GetType().Name)
    Assert.StartsWith("UnionConverter`1", converterOption.GetType().Name)
    Assert.Equal<byte>(Encode converterItem value, Encode converterOption optionValue)
    Assert.Equal<byte>(EncodeAuto converterItem value, EncodeAuto converterOption optionValue)
    Assert.Equal<byte>(Encode converterItem null, Encode converterOption null)
    Assert.Equal<byte>(EncodeAuto converterItem null, EncodeAuto converterOption null)
    ()

[<Fact>]
let ``Nullable With FSharp Option Of Int32`` () = Comparison (box 10) (box (Some 10))

[<Fact>]
let ``Nullable With FSharp Option Of Int64`` () = Comparison (box Int64.MaxValue) (box (Some Int64.MaxValue))

[<Fact>]
let ``Nullable With FSharp Option Of Value Tuple With Int32 And String`` () =
    Comparison (box (struct (-1536, "Inner text"))) (box (Some(struct (-1536, "Inner text"))))

[<Fact>]
let ``Nullable With FSharp Option Of Value Tuple With String And Guid`` () =
    let value = struct ("Value tuple", Guid.Parse("ca86e602-eaae-4653-b74b-fd1dd248f620"))
    Comparison (box value) (box (Some value))
