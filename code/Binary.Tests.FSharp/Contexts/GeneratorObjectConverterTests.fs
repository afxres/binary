namespace Contexts

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Text
open Xunit

type FakeObjectConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (InvalidOperationException("Encode"))

    override __.EncodeAuto(_, _) = raise (InvalidOperationException("EncodeAuto"))

    override __.EncodeWithLengthPrefix(_, _) = raise (InvalidOperationException("EncodeWithLengthPrefix"))

    override __.Encode(_) = raise (InvalidOperationException("EncodeBuffer"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotImplementedException())

    override __.DecodeAuto _ = raise (NotImplementedException())

    override __.DecodeWithLengthPrefix _ = raise (NotImplementedException())

    override __.Decode(_ : byte array) : 'T = raise (NotImplementedException())

type FakeStringConverter() =
    inherit Converter<string>()

    override __.Encode(allocator, item) = Allocator.Append(&allocator, item.AsSpan(), Encoding.UTF8)

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : string = Encoding.UTF8.GetString span

type FakeObjectGenerator(converters : IReadOnlyDictionary<Type, IConverter>) =
    interface IGenerator with
        member __.GetConverter t =
            converters.[t]

type GeneratorObjectConverterTests() =
    static let CreateConverter() =
        let sequences = [
            typeof<int>, FakeObjectConverter<int>() :> IConverter;
            typeof<string>, FakeStringConverter() :> IConverter;
        ]
        let generator = sequences |> readOnlyDict |> FakeObjectGenerator
        let converterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorObjectConverter") |> Array.exactlyOne
        let converter = Activator.CreateInstance(converterType, [| generator |> box |]) :?> Converter<obj>
        converter

    static let rec InvokeWithInsufficientExecutionStack (action : Action) =
        let status = try RuntimeHelpers.EnsureSufficientExecutionStack(); None with :? InsufficientExecutionStackException as x -> Some x
        match status with
        | None ->
            InvokeWithInsufficientExecutionStack action
        | _ ->
            action.Invoke()
        ()

    static member ``Encode Arguments`` : (obj array) seq = seq {
        yield [| Action<obj>(fun x -> let mutable allocator = Allocator() in CreateConverter().Encode(&allocator, x)); "Encode" |]
        yield [| Action<obj>(fun x -> let mutable allocator = Allocator() in CreateConverter().EncodeAuto(&allocator, x)); "EncodeWithLengthPrefix" |]
        yield [| Action<obj>(fun x -> let mutable allocator = Allocator() in CreateConverter().EncodeWithLengthPrefix(&allocator, x)); "EncodeWithLengthPrefix" |]
        yield [| Action<obj>(fun x -> CreateConverter().Encode x |> ignore); "EncodeBuffer" |]
    }

    static member ``Decode Arguments`` : (obj array) seq = seq {
        yield [| Func<obj>(fun () -> let mutable span = ReadOnlySpan() in CreateConverter().Decode &span); "Decode(System.ReadOnlySpan`1[System.Byte] ByRef)" |]
        yield [| Func<obj>(fun () -> let mutable span = ReadOnlySpan([| 0uy |]) in CreateConverter().DecodeAuto &span); "DecodeAuto" |]
        yield [| Func<obj>(fun () -> let mutable span = ReadOnlySpan([| 0uy |]) in CreateConverter().DecodeWithLengthPrefix &span); "DecodeWithLengthPrefix" |]
        yield [| Func<obj>(fun () -> CreateConverter().Decode Array.empty); "Decode(Byte[])" |]
    }

    [<Fact>]
    member __.``Length`` () =
        let generator = Generator.CreateDefault()
        let alpha = generator.GetConverter<obj>()
        let bravo = CreateConverter()
        Assert.Equal(alpha.GetType(), bravo.GetType())
        Assert.Equal(0, alpha.Length)
        Assert.Equal(0, bravo.Length)
        ()

    [<Fact>]
    member __.``Generator Get Object Converter`` () =
        let generator = Generator.CreateDefault()
        let converter = generator.GetConverter<obj>()
        let t = converter.GetType()
        Assert.Equal("GeneratorObjectConverter", t.Name)
        ()

    [<Theory>]
    [<MemberData("Encode Arguments")>]
    member __.``Encode Null`` (action : Action<obj>, _ : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> action.Invoke null)
        let message = "Can not get type of null object."
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Encode Arguments")>]
    member __.``Encode Object Instance`` (action : Action<obj>, _ : string) =
        let error = Assert.Throws<NotSupportedException>(fun () -> action.Invoke(obj()))
        let message = "Can not encode object, type: System.Object"
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Encode Arguments")>]
    member __.``Encode (ensure override)`` (action : Action<obj>, message : string) =
        let error = Assert.Throws<InvalidOperationException>(fun () -> action.Invoke(box 0))
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Encode Arguments")>]
    member __.``Encode Ensure Sufficient Execution Stack`` (action : Action<obj>, _ : string) =
        let action = Action(fun () -> Assert.Throws<InsufficientExecutionStackException>(fun () -> action.Invoke(box 0)) |> ignore)
        InvokeWithInsufficientExecutionStack action
        ()

    [<Theory>]
    [<MemberData("Decode Arguments")>]
    member __.``Decode (ensure override)`` (action : Func<obj>, name : string) =
        let token = Assert.IsType<Token>(action.Invoke())
        Assert.Empty token.Children
        Assert.Equal(0, token.Memory.Length)

        let converterType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorObjectConverter") |> Array.exactlyOne
        let methods = converterType.GetMethods()
        let method = Assert.Single(methods, fun x -> x.ToString().Contains(name))
        Assert.StartsWith("Decode", method.Name)
        Assert.True(method.IsVirtual)
        Assert.Equal(converterType, method.DeclaringType)
        Assert.Equal(converterType, method.ReflectedType)
        ()
