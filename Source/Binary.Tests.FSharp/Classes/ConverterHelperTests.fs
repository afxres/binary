namespace Classes

open Mikodev.Binary
open System
open Xunit

type FakeConverter () =
    interface IConverter with
        member __.Decode(span: inref<ReadOnlySpan<byte>>): obj = raise (System.NotImplementedException())

        member __.Decode(buffer: byte []): obj = raise (System.NotImplementedException())

        member __.DecodeAuto(span: byref<ReadOnlySpan<byte>>): obj = raise (System.NotImplementedException())

        member __.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>): obj = raise (System.NotImplementedException())

        member __.Encode(item: obj): byte [] = raise (System.NotImplementedException())

        member __.Encode(allocator: byref<Allocator>, item: obj): unit = raise (System.NotImplementedException())

        member __.EncodeAuto(allocator: byref<Allocator>, item: obj): unit = raise (System.NotImplementedException())

        member __.EncodeWithLengthPrefix(allocator: byref<Allocator>, item: obj): unit = raise (System.NotImplementedException())

        member __.Length: int = raise (System.NotImplementedException())

type GoodConverter<'T> () =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type ConverterHelperTests() =
    [<Fact>]
    member __.``Get Generic Argument (converter null)`` () =
        let error = Assert.Throws<ArgumentNullException>(fun () -> ConverterHelper.GetGenericArgument(Unchecked.defaultof<IConverter>) |> ignore)
        let methodInfo = typeof<ConverterHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "GetGenericArgument" && x.GetParameters().[0].ParameterType = typeof<IConverter>) |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("converter", parameter.Name)
        Assert.Equal("converter", error.ParamName)
        ()

    [<Fact>]
    member __.``Get Generic Argument (type null)`` () =
        let error = Assert.Throws<ArgumentNullException>(fun () -> ConverterHelper.GetGenericArgument(Unchecked.defaultof<Type>) |> ignore)
        let methodInfo = typeof<ConverterHelper>.GetMethods() |> Array.filter (fun x -> x.Name = "GetGenericArgument" && x.GetParameters().[0].ParameterType = typeof<Type>) |> Array.exactlyOne
        let parameter = methodInfo.GetParameters() |> Array.last
        Assert.Equal("type", parameter.Name)
        Assert.Equal("type", error.ParamName)
        ()

    static member ``Data Invalid Converter`` : (obj array) seq =
        seq {
            yield [| new FakeConverter() |]
        }

    [<Theory>]
    [<MemberData("Data Invalid Converter")>]
    member __.``Get Generic Argument (invalid converter instance)`` (converter : IConverter) =
        let error = Assert.Throws<ArgumentException>(fun () -> ConverterHelper.GetGenericArgument converter |> ignore)
        let message = sprintf "Invalid type, '%O' is not a subclass of '%O'" (converter.GetType()) typedefof<Converter<_>>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Invalid Type`` : (obj array) seq =
        seq {
            yield [| typeof<FakeConverter> |]
            yield [| typeof<IConverter> |]
            yield [| typeof<Converter<int>> |]
            yield [| typeof<obj> |]
        }

    [<Theory>]
    [<MemberData("Data Invalid Type")>]
    member __.``Get Generic Argument (invalid converter type)`` (t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> ConverterHelper.GetGenericArgument t |> ignore)
        let message = sprintf "Invalid type, '%O' is not a subclass of '%O'" t typedefof<Converter<_>>
        Assert.Null(error.ParamName)
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Converter`` : (obj array) seq =
        seq {
            yield [| new GoodConverter<int>(); box typeof<int> |]
            yield [| new GoodConverter<obj>(); box typeof<obj> |]
        }

    [<Theory>]
    [<MemberData("Data Converter")>]
    member __.``Get Generic Argument (valid)`` (converter : IConverter, t : Type) =
        let alpha = ConverterHelper.GetGenericArgument(converter)
        let bravo = ConverterHelper.GetGenericArgument(converter.GetType())
        Assert.Equal(t, alpha)
        Assert.Equal(t, bravo)
        ()
