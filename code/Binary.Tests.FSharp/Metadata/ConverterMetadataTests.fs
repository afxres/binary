namespace Metadata

open Mikodev.Binary
open System
open System.Reflection
open Xunit
open System.Linq.Expressions

type MetadataFakeConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise(NotSupportedException())

    override __.EncodeAuto(_, _) = raise (NotSupportedException())

type ConverterMetadataTests() =
    member private __.GetInterfaceMethod() =
        let interfaceType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "IConverterMetadata") |> Array.exactlyOne
        let interfaceMethod = interfaceType.GetMethods() |> Array.filter (fun x -> x.Name.StartsWith "GetMethod") |> Array.exactlyOne
        let converter = Expression.Parameter(typeof<obj>, "converter")
        let name = Expression.Parameter(typeof<string>, "name")
        let expression = Expression.Call(Expression.Convert(converter, interfaceType), interfaceMethod, Array.singleton (name :> Expression))
        let functor = Expression.Lambda<Func<obj, string, MethodInfo>>(expression, [| converter; name |])
        functor.Compile()

    [<Theory>]
    [<InlineData("Encode")>]
    [<InlineData("Decode")>]
    [<InlineData("EncodeAuto")>]
    member me.``Get Method For Override`` (methodName : string) =
        let interfaceMethod = me.GetInterfaceMethod()
        let converter = MetadataFakeConverter<obj>()
        let method = interfaceMethod.Invoke(converter, methodName)
        Assert.Equal(methodName, method.Name)
        Assert.Equal(converter.GetType(), method.ReflectedType)
        Assert.Equal(converter.GetType(), method.DeclaringType)
        ()

    [<Theory>]
    [<InlineData("DecodeAuto")>]
    [<InlineData("DecodeWithLengthPrefix")>]
    [<InlineData("EncodeWithLengthPrefix")>]
    member me.``Get Method For Non Override`` (methodName : string) =
        let interfaceMethod = me.GetInterfaceMethod()
        let converter = MetadataFakeConverter<obj>()
        let method = interfaceMethod.Invoke(converter, methodName)
        Assert.Equal(methodName, method.Name)
        Assert.Equal(typeof<Converter<obj>>, method.ReflectedType)
        Assert.Equal(typeof<Converter<obj>>, method.DeclaringType)
        ()

    [<Theory>]
    [<InlineData(null)>]
    [<InlineData("")>]
    [<InlineData("Length")>]
    [<InlineData("GetGenericArgument")>]
    member me.``Get Method Invalid Name`` (name : string) =
        let interfaceMethod = me.GetInterfaceMethod()
        let converter = MetadataFakeConverter<obj>()
        let error = Assert.Throws<ArgumentException>(fun () -> interfaceMethod.Invoke(converter, name) |> ignore)
        let message = $"Invalid method name '{name}'"
        Assert.Equal("name", error.ParamName)
        Assert.StartsWith(message, error.Message)
        ()
