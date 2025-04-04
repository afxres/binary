﻿module Contexts.GeneratorTests

open Mikodev.Binary
open System
open System.Collections
open Xunit

let generator = Generator.CreateDefault()

type EmptyStructure = struct end

[<Fact>]
let ``Get Converter (via type)`` () =
    let alpha = generator.GetConverter(typeof<int * string>)
    Assert.IsAssignableFrom<Converter<int * string>>(alpha) |> ignore
    ()

[<Fact>]
let ``Get Converter (generic)`` () =
    let alpha = generator.GetConverter<string * int>()
    Assert.IsAssignableFrom<Converter<string * int>>(alpha) |> ignore
    ()

[<Fact>]
let ``Get Converter (anonymous)`` () =
    let anonymous = {| id = 0; data = "1024" |}
    let alpha = generator.GetConverter(anonymous)
    Assert.NotNull alpha
    ()

[<Fact>]
let ``Get Converter (via type, argument null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> generator.GetConverter null |> ignore)
    let parameter = typeof<IGenerator>.GetMethod("GetConverter").GetParameters() |> Array.exactlyOne
    Assert.Equal("type", error.ParamName)
    Assert.Equal("type", parameter.Name)
    ()

[<Fact>]
let ``Get Converter (static type)`` () =
    let t = typeof<Tuple>
    Assert.True(t.IsAbstract && t.IsSealed)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid static type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (pointer type)`` () =
    let t = typeof<EmptyStructure>.MakePointerType()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    let message = sprintf "Invalid pointer type: %O" t
    Assert.Null(error.ParamName)
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Get Converter (byref type)`` () =
    let t = typeof<int>.MakeByRefType()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid byref type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (byref-like type)`` () =
    let t = typedefof<Memory<_>>.Assembly.GetType("System.Span`1").MakeGenericType(typeof<int>)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid byref-like type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (byref-like type definition)`` () =
    let t = typedefof<Memory<_>>.Assembly.GetType("System.Span`1")
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid byref-like type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (generic type definition)`` () =
    let t = typedefof<Tuple<_>>
    Assert.True(t.IsGenericTypeDefinition)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid generic type definition: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (generic type parameter)`` () =
    let definition = typedefof<_ list>
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter definition |> ignore)
    Assert.Null(error.ParamName)
    let message = sprintf "Invalid generic type definition: %O" definition
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Get Converter (non-generic collection)`` () =
    let types = [| typeof<IEnumerable>; typeof<Stack>; typeof<Queue> |]
    for i in types do
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter i |> ignore)
        Assert.Null error.ParamName
        let message = sprintf "Invalid non-generic collection type: %O" i
        Assert.Equal(message, error.Message)
        ()
    ()

[<Fact>]
let ``Encode (obj, instance)`` () =
    let source = obj ()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Encode source |> ignore)
    Assert.Equal("Can not encode object, type: System.Object", error.Message)
    ()

[<Fact>]
let ``Encode (obj, null)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Encode<obj> null |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal("Can not get type of null object.", error.Message)
    ()

[<Theory>]
[<InlineData(43, 4)>]
[<InlineData("magic", 5)>]
let ``Encode (via type)`` (item: obj, count: int) =
    let buffer = generator.Encode(item, item.GetType())
    Assert.Equal(count, buffer.Length)
    ()

[<Fact>]
let ``Encode (via type, null)`` () =
    let buffer = generator.Encode(null, typeof<Uri>)
    Assert.Equal(0, buffer.Length)
    ()

[<Theory>]
[<InlineData(3.14)>]
[<InlineData("pi")>]
let ``Encode (as object)`` (value: obj) =
    let source = box value
    let buffer = generator.Encode<obj> source
    Assert.NotEmpty(buffer)
    ()

[<Fact>]
let ``Decode (as object)`` () =
    let token = Assert.IsType<Token>(generator.Decode<obj> Array.empty<byte>)
    Assert.Empty token.Children
    Assert.Equal(0, token.Memory.Length)
    ()

[<Theory>]
[<InlineData(2.71)>]
[<InlineData("e")>]
let ``Decode (via type)`` (value: obj) =
    let buffer = generator.Encode value
    let result = generator.Decode(buffer, value.GetType())
    Assert.Equal(value, result)
    ()

[<Fact>]
let ``Decode (via type, argument null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> generator.Decode(Array.empty<byte>, Unchecked.defaultof<Type>) |> ignore)
    Assert.Equal("type", error.ParamName)
    ()

[<Theory>]
[<InlineData(6)>]
[<InlineData("fox")>]
let ``Encode and Decode``<'a> (data: 'a) =
    let buffer = generator.Encode data
    let result = generator.Decode<'a> buffer
    Assert.Equal<'a>(data, result)
    ()

[<Fact>]
let ``Internal Types`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Decode<Converter<obj>> Array.empty<byte> |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid internal type: %O" typeof<Converter<obj>>, error.Message)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Decode<Token> Array.empty<byte> |> ignore)
    Assert.Null(error.ParamName)
    Assert.Equal(sprintf "Invalid internal type: %O" typeof<Token>, error.Message)
    ()

// Bad creator operation
type BadType = { some: obj }

type BadConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, ``type``) =
            if ``type`` = typeof<BadType> then
                context.GetConverter typeof<int> // bad return value
            else
                null

type BadConverterInterfaceImplementation() =
    interface IConverter with
        member __.Decode(span: inref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.Decode(_: byte[]) : obj = raise (NotSupportedException())

        member __.DecodeAuto(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.Encode(_: obj) : byte[] = raise (NotSupportedException())

        member __.Encode(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.EncodeAuto(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.EncodeWithLengthPrefix(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.Length: int = raise (NotSupportedException())

type BadConverterCreatorInterfaceImplementation() =
    interface IConverterCreator with
        member __.GetConverter(_, ``type``) =
            if ``type`` = typeof<BadType> then
                BadConverterInterfaceImplementation() :> IConverter // bad return value
            else
                null

[<Fact>]
let ``Bad Creator (item type mismatch)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverterCreator(BadConverterCreator()).Build()
    let error = Assert.Throws<InvalidOperationException>(fun () -> generator.GetConverter typeof<BadType> |> ignore)
    let message =
        $"Invalid converter, expected: converter for '{typeof<BadType>}', actual: {generator.GetConverter<int>().GetType()}, converter creator type: {typeof<BadConverterCreator>}"
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``Bad Creator (not a subclass)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverterCreator(BadConverterCreatorInterfaceImplementation()).Build()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter typeof<BadType> |> ignore)
    let message = $"Invalid converter instance, type: {typeof<BadConverterInterfaceImplementation>}"
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let generator = Generator.CreateDefault()
    Assert.Matches(@"Converter Count = 1, Converter Creator Count = \d+", generator.ToString())
    ()
