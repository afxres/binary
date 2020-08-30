module Contexts.GeneratorObjectConverterTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type FakeConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (InvalidOperationException("Encode"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (InvalidOperationException("Decode"))

    override __.EncodeAuto(_, _) = raise (InvalidOperationException("Encode Auto"))

    override __.DecodeAuto _ = raise (InvalidOperationException("Decode Auto"))

    override __.EncodeWithLengthPrefix(_, _) = raise (InvalidOperationException("Encode With Length Prefix"))

    override __.DecodeWithLengthPrefix _ = raise (InvalidOperationException("Decode With Length Prefix"))

    override __.Encode(_) = raise (InvalidOperationException("Encode Buffer"))

    override __.Decode(_ : byte array) : 'T = raise (InvalidOperationException("Decode Buffer"))

type FakeGenerator(converters : IReadOnlyDictionary<Type, IConverter>) =
    interface IGenerator with
        member __.GetConverter t =
            converters.[t]

let CreateConverter() =
    let converterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorObjectConverter") |> Array.exactlyOne
    let converter = Activator.CreateInstance(converterType, [| Unchecked.defaultof<obj> |]) :?> Converter<obj>
    converter

[<Fact>]
let ``Length`` () =
    let generator = Generator.CreateDefault()
    let alpha = generator.GetConverter<obj>()
    let bravo = CreateConverter()
    Assert.Equal(alpha.GetType(), bravo.GetType())
    Assert.Equal(0, alpha.Length)
    Assert.Equal(0, bravo.Length)
    ()

[<Fact>]
let ``Generator Get Object Converter`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<obj>()
    let t = converter.GetType()
    Assert.Equal("GeneratorObjectConverter", t.Name)
    ()

[<Fact>]
let ``Encode Null`` () =
    let converter = CreateConverter()
    let alpha = Assert.Throws<ArgumentException>(fun () -> let mutable allocator = Allocator() in converter.Encode(&allocator, null) |> ignore)
    let bravo = Assert.Throws<ArgumentException>(fun () -> converter.Encode null |> ignore)
    let message = "Can not get type of null object."
    Assert.Null(alpha.ParamName)
    Assert.Null(bravo.ParamName)
    Assert.Equal(message, alpha.Message)
    Assert.Equal(message, bravo.Message)
    ()

[<Fact>]
let ``Encode Object Instance`` () =
    let converter = CreateConverter()
    let alpha = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in converter.Encode(&allocator, obj()) |> ignore)
    let bravo = Assert.Throws<NotSupportedException>(fun () -> converter.Encode(obj()) |> ignore)
    let message = "Can not encode object, type: System.Object"
    Assert.Equal(message, alpha.Message)
    Assert.Equal(message, bravo.Message)
    ()

let TestEncode (message : string) (action : Converter<obj> -> unit) =
    let converterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorObjectConverter") |> Array.exactlyOne
    let converter = Activator.CreateInstance(converterType, [| [ typeof<int>, FakeConverter<int>() :> IConverter ] |> readOnlyDict |> FakeGenerator |> box |]) :?> Converter<obj>
    let alpha = Assert.Throws<InvalidOperationException>(fun () -> action converter)
    Assert.Equal(message, alpha.Message)
    ()

[<Fact>]
let ``Encode`` () =
    TestEncode "Encode" (fun (converter : Converter<obj>) -> let mutable allocator = Allocator() in converter.Encode(&allocator, 0))
    ()

[<Fact>]
let ``Encode Auto`` () =
    TestEncode "Encode With Length Prefix" (fun (converter : Converter<obj>) -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, 0))
    ()

[<Fact>]
let ``Encode With Length Prefix`` () =
    TestEncode "Encode With Length Prefix" (fun (converter : Converter<obj>) -> let mutable allocator = Allocator() in converter.EncodeWithLengthPrefix(&allocator, 0))
    ()

[<Fact>]
let ``Encode Buffer`` () =
    TestEncode "Encode Buffer" (fun (converter : Converter<obj>) -> converter.Encode 0 |> ignore)
    ()

let TestDecode (action : Converter<obj> -> unit) =
    let converterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorObjectConverter") |> Array.exactlyOne
    let converter = Activator.CreateInstance(converterType, [| Unchecked.defaultof<obj> |]) :?> Converter<obj>
    let alpha = Assert.Throws<NotSupportedException>(fun () -> action converter)
    let message = "Can not decode object, type: System.Object"
    Assert.Equal(message, alpha.Message)

    let methods = converterType.GetMethods() |> Array.filter (fun x -> x.Name.StartsWith "Encode")
    Assert.Equal(4, methods.Length)
    Assert.All(methods, fun x -> Assert.True(x.IsVirtual && x.DeclaringType = converterType && x.ReflectedType = converterType))
    ()

[<Fact>]
let ``Decode`` () =
    TestDecode (fun converter -> let span = ReadOnlySpan<byte>() in converter.Decode &span |> ignore)
    ()

[<Fact>]
let ``Decode Auto`` () =
    TestDecode (fun converter -> let mutable span = ReadOnlySpan<byte>() in converter.DecodeAuto &span |> ignore)
    ()

[<Fact>]
let ``Decode With Length Prefix`` () =
    TestDecode (fun converter -> let mutable span = ReadOnlySpan<byte>() in converter.DecodeWithLengthPrefix &span |> ignore)
    ()

[<Fact>]
let ``Decode Buffer`` () =
    TestDecode (fun converter -> converter.Decode Array.empty<byte> |> ignore)
    ()
