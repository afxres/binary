module TupleLike.ValueTupleAdvanceTests

open Mikodev.Binary
open System
open Xunit

type TestConverter<'a>(text: string, list: ResizeArray<string>) =
    inherit Converter<'a>(0)

    override __.Encode(_, _) =
        list.Add(sprintf "%s e" text)
        ()

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'a =
        list.Add(sprintf "%s d" text)
        Unchecked.defaultof<'a>

    override __.EncodeAuto(_, _) =
        list.Add(sprintf "%s ea" text)
        ()

    override __.DecodeAuto _ =
        list.Add(sprintf "%s da" text)
        Unchecked.defaultof<'a>

[<Fact>]
let ``Value Tuple Expand`` () =
    let list = ResizeArray<string>()
    let generator =
        Generator
            .CreateDefaultBuilder()
            .AddConverter(TestConverter<int>("int", list))
            .AddConverter(TestConverter<string>("string", list))
            .AddConverter(TestConverter<single>("single", list))
            .AddConverter(TestConverter<double>("double", list))
            .Build()
    let source = struct (0, "1", struct (single 2, double 3))
    let converter = generator.GetConverter(anonymous = source)
    let buffer = converter.Encode source
    Assert.Empty buffer
    Assert.Equal<string>([| "int ea"; "string ea"; "single ea"; "double e" |], list)

    list.Clear()
    let result = converter.Decode Array.empty
    Assert.Equal(Unchecked.defaultof<struct (int * string * struct (single * double))>, result)
    Assert.Equal<string>([| "int da"; "string da"; "single da"; "double d" |], list)
    ()

[<Fact>]
let ``Value Tuple Expand Limited`` () =
    let list = ResizeArray<string>()
    let generator =
        Generator
            .CreateDefaultBuilder()
            .AddConverter(TestConverter<int>("int", list))
            .AddConverter(TestConverter<string>("string", list))
            .AddConverter(TestConverter<single>("single", list))
            .AddConverter(TestConverter<double>("double", list))
            .AddConverter(TestConverter<struct (single * double)>("value (f32, f64)", list))
            .Build()
    let source = struct (0, "1", struct (single 2, double 3))
    let converter = generator.GetConverter(anonymous = source)
    let buffer = converter.Encode source
    Assert.Empty buffer
    Assert.Equal<string>([| "int ea"; "string ea"; "single ea"; "double e" |], list)

    list.Clear()
    let result = converter.Decode Array.empty
    Assert.Equal(Unchecked.defaultof<struct (int * string * struct (single * double))>, result)
    Assert.Equal<string>([| "int da"; "string da"; "single da"; "double d" |], list)
    ()
