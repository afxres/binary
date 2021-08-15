namespace Contexts

open Mikodev.Binary
open System
open Xunit

type FakeConverter () =
    interface IConverter with
        member __.Decode(span: inref<ReadOnlySpan<byte>>): obj = raise (NotImplementedException())

        member __.Decode(buffer: byte []): obj = raise (NotImplementedException())

        member __.DecodeAuto(span: byref<ReadOnlySpan<byte>>): obj = raise (NotImplementedException())

        member __.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>): obj = raise (NotImplementedException())

        member __.Encode(item: obj): byte [] = raise (NotImplementedException())

        member __.Encode(allocator: byref<Allocator>, item: obj): unit = raise (NotImplementedException())

        member __.EncodeAuto(allocator: byref<Allocator>, item: obj): unit = raise (NotImplementedException())

        member __.EncodeWithLengthPrefix(allocator: byref<Allocator>, item: obj): unit = raise (NotImplementedException())

        member __.Length: int = raise (NotImplementedException())

type GoodConverter<'T> () =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type ConverterModuleTests() =
    [<Fact>]
    member __.``Get Generic Argument (converter null)`` () =
        let error = Assert.Throws<ArgumentNullException>(fun () -> Converter.GetGenericArgument(Unchecked.defaultof<IConverter>) |> ignore)
        let methodInfo = typeof<Converter>.GetMethods() |> Array.filter (fun x -> x.Name = "GetGenericArgument" && x.GetParameters().[0].ParameterType = typeof<IConverter>) |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("converter", parameter.Name)
        Assert.Equal("converter", error.ParamName)
        ()

    [<Fact>]
    member __.``Get Generic Argument (type null)`` () =
        let error = Assert.Throws<ArgumentNullException>(fun () -> Converter.GetGenericArgument(Unchecked.defaultof<Type>) |> ignore)
        let methodInfo = typeof<Converter>.GetMethods() |> Array.filter (fun x -> x.Name = "GetGenericArgument" && x.GetParameters().[0].ParameterType = typeof<Type>) |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("type", parameter.Name)
        Assert.Equal("type", error.ParamName)
        ()

    static member ``Data Invalid Converter`` : (obj array) seq = seq {
        yield [| FakeConverter() |]
    }

    [<Theory>]
    [<MemberData("Data Invalid Converter")>]
    member __.``Get Generic Argument (invalid converter instance)`` (converter : IConverter) =
        let error = Assert.Throws<ArgumentException>(fun () -> Converter.GetGenericArgument converter |> ignore)
        let message = sprintf "Can not get generic argument, '%O' is not a subclass of '%O'" (converter.GetType()) typedefof<Converter<_>>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Invalid Type`` : (obj array) seq = seq {
        yield [| typeof<FakeConverter> |]
        yield [| typeof<IConverter> |]
        yield [| typeof<Converter<int>> |]
        yield [| typeof<obj> |]
    }

    [<Theory>]
    [<MemberData("Data Invalid Type")>]
    member __.``Get Generic Argument (invalid converter type)`` (t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> Converter.GetGenericArgument t |> ignore)
        let message = sprintf "Can not get generic argument, '%O' is not a subclass of '%O'" t typedefof<Converter<_>>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Converter With Type`` : (obj array) seq = seq {
        yield [| GoodConverter<int>(); box typeof<int> |]
        yield [| GoodConverter<obj>(); box typeof<obj> |]
    }

    [<Theory>]
    [<MemberData("Data Converter With Type")>]
    member __.``Get Generic Argument (valid)`` (converter : IConverter, t : Type) =
        let alpha = Converter.GetGenericArgument(converter)
        let bravo = Converter.GetGenericArgument(converter.GetType())
        Assert.Equal(t, alpha)
        Assert.Equal(t, bravo)
        ()

    static member ``Data Converter`` : (obj array) seq = seq {
        yield [| GoodConverter<int>() |]
        yield [| GoodConverter<obj>() |]
    }

    [<Theory>]
    [<MemberData("Data Converter")>]
    member __.``Get Method`` (converter : IConverter) =
        let encode = [|
            "Encode";
            "EncodeAuto";
            "EncodeWithLengthPrefix";
        |]
        let decode = [|
            "Decode";
            "DecodeAuto";
            "DecodeWithLengthPrefix";
        |]
        for i in encode do
            let m = Converter.GetMethod(converter, i)
            Assert.NotNull m
            Assert.Equal(i, m.Name)
            let parameters = m .GetParameters()
            Assert.Equal(2, parameters.Length)
            Assert.Equal("Allocator&", parameters.[0].ParameterType.Name)
        for i in decode do
            let m = Converter.GetMethod(converter, i)
            Assert.NotNull m
            Assert.Equal(i, m.Name)
            let parameters = m .GetParameters()
            Assert.Equal(1, parameters.Length)
            Assert.Equal("ReadOnlySpan`1&", parameters.[0].ParameterType.Name)
        ()
