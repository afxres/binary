namespace Contexts

open Mikodev.Binary
open System
open Xunit

type FakeConverter() =
    interface IConverter with
        member __.Decode(span: inref<ReadOnlySpan<byte>>) : obj = raise (NotImplementedException())

        member __.Decode(_: byte[]) : obj = raise (NotImplementedException())

        member __.DecodeAuto(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotImplementedException())

        member __.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotImplementedException())

        member __.Encode(_: obj) : byte[] = raise (NotImplementedException())

        member __.Encode(allocator: byref<Allocator>, _: obj) : unit = raise (NotImplementedException())

        member __.EncodeAuto(allocator: byref<Allocator>, _: obj) : unit = raise (NotImplementedException())

        member __.EncodeWithLengthPrefix(allocator: byref<Allocator>, _: obj) : unit = raise (NotImplementedException())

        member __.Length: int = raise (NotImplementedException())

type GoodConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type ConverterModuleTests() =
    [<Fact>]
    member __.``Get Generic Argument (converter null)``() =
        let error = Assert.Throws<ArgumentNullException>(fun () -> Converter.GetGenericArgument(Unchecked.defaultof<IConverter>) |> ignore)
        let methodInfo = typeof<Converter>.GetMethods() |> Array.filter (fun x -> x.Name = "GetGenericArgument") |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("converter", parameter.Name)
        Assert.Equal("converter", error.ParamName)
        ()

    static member ``Data Invalid Converter``: (obj array) seq = seq { yield [| FakeConverter() |] }

    [<Theory>]
    [<MemberData("Data Invalid Converter")>]
    member __.``Get Generic Argument (invalid converter instance)``(converter: IConverter) =
        let error = Assert.Throws<ArgumentException>(fun () -> Converter.GetGenericArgument converter |> ignore)
        let message = sprintf "Invalid converter instance, type: %O" (converter.GetType())
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Converter With Type``: (obj array) seq = seq {
        yield [| GoodConverter<int>(); box typeof<int> |]
        yield [| GoodConverter<obj>(); box typeof<obj> |]
    }

    [<Theory>]
    [<MemberData("Data Converter With Type")>]
    member __.``Get Generic Argument (valid)``(converter: IConverter, t: Type) =
        let a = Converter.GetGenericArgument(converter)
        Assert.Equal(t, a)
        ()

    static member ``Data Converter``: (obj array) seq = seq {
        yield [| GoodConverter<int>() |]
        yield [| GoodConverter<obj>() |]
    }

    [<Theory>]
    [<MemberData("Data Converter")>]
    member __.``Get Method``(converter: IConverter) =
        let encode = [| "Encode"; "EncodeAuto"; "EncodeWithLengthPrefix" |]
        let decode = [| "Decode"; "DecodeAuto"; "DecodeWithLengthPrefix" |]
        for i in encode do
            let m = Converter.GetMethod(converter, i)
            Assert.NotNull m
            Assert.Equal(i, m.Name)
            let parameters = m.GetParameters()
            Assert.Equal(2, parameters.Length)
            Assert.Equal("Allocator&", parameters.[0].ParameterType.Name)
        for i in decode do
            let m = Converter.GetMethod(converter, i)
            Assert.NotNull m
            Assert.Equal(i, m.Name)
            let parameters = m.GetParameters()
            Assert.Equal(1, parameters.Length)
            Assert.Equal("ReadOnlySpan`1&", parameters.[0].ParameterType.Name)
        ()

    [<Fact>]
    member __.``Get Method (converter null)``() =
        let error = Assert.Throws<ArgumentNullException>(fun () -> Converter.GetMethod(Unchecked.defaultof<IConverter>, String.Empty) |> ignore)
        let methodInfo = typeof<Converter>.GetMethods() |> Array.filter (fun x -> x.Name = "GetMethod") |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.head
        Assert.Equal("converter", parameter.Name)
        Assert.Equal("converter", error.ParamName)
        ()

    [<Theory>]
    [<MemberData("Data Invalid Converter")>]
    member __.``Get Method (invalid converter instance)``(converter: IConverter) =
        let error = Assert.Throws<ArgumentException>(fun () -> Converter.GetMethod(converter, String.Empty) |> ignore)
        let message = sprintf "Invalid converter instance, type: %O" (converter.GetType())
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("Bad Name")>]
    member __.``Get Method (invalid name)``(name: string) =
        let error = Assert.Throws<ArgumentException>(fun () -> Converter.GetMethod(GoodConverter<int>(), name) |> ignore)
        let message = $"Invalid method name '{name}'"
        let methodInfo = typeof<Converter>.GetMethods() |> Array.filter (fun x -> x.Name = "GetMethod") |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("name", parameter.Name)
        Assert.Equal("name", error.ParamName)
        Assert.StartsWith(message, error.Message)
        ()
