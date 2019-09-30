module Classes.GeneratorTests

open Mikodev.Binary
open System
open Xunit

let generator = new Generator()

[<Fact>]
let ``Constructor (argument null or empty)`` () =
    let test (generator : Generator) =
        let source = struct (768, "data")
        let buffer = generator.ToBytes source
        let result = generator.ToValue<struct (int * string)> buffer
        Assert.Equal(source, result)

    test(new Generator())
    test(new Generator(null))
    test(new Generator(Seq.empty<Converter>))
    test(new Generator([ null ]))
    ()
    
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
let ``Get Converter (via type, argument null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> generator.GetConverter(null) |> ignore)
    Assert.Equal("type", error.ParamName)
    ()

[<Fact>]
let ``Get Converter (static type)`` () =
    let t = typeof<Tuple>
    Assert.True(t.IsAbstract && t.IsSealed)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    Assert.Equal(sprintf "Invalid static type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (byref-like type)`` () =
    let t = Type.GetType("System.Span`1").MakeGenericType(typeof<int>)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    Assert.Equal(sprintf "Invalid byref-like type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (byref-like type definition)`` () =
    let t = Type.GetType("System.Span`1")
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    Assert.Equal(sprintf "Invalid byref-like type: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (generic type definition)`` () =
    let t = typedefof<Tuple<_>>
    Assert.True(t.IsGenericTypeDefinition)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    Assert.Equal(sprintf "Invalid generic type definition: %O" t, error.Message)
    ()

[<Fact>]
let ``Get Converter (generic type parameter)`` () =
    let definition = typedefof<_ list>
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter definition |> ignore)
    Assert.Contains("Invalid generic type definition", error.Message)
    ()

[<Fact>]
let ``Get Converter (IConverter interface)`` () =
    let source = generator :> IGenerator
    let a = source.GetConverter(typeof<int>)
    let b = source.GetConverter(typeof<string>)
    Assert.Equal(typeof<int>, a.ItemType)
    Assert.Equal(typeof<string>, b.ItemType)
    ()

[<Fact>]
let ``To Bytes (obj, instance)`` () =
    let source = new obj()
    let error = Assert.Throws<ArgumentException>(fun () -> generator.ToBytes source |> ignore)
    Assert.Contains("Invalid type", error.Message)
    ()

[<Fact>]
let ``To Bytes (obj, null)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.ToBytes<obj> null |> ignore)
    Assert.Contains("Can not get type of null object.", error.Message)
    ()

[<Theory>]
[<InlineData(43, 4)>]
[<InlineData("magic", 5)>]
let ``To Bytes (via type)`` (item : obj, count : int) =
    let buffer = generator.ToBytes(item, item.GetType())
    Assert.Equal(count, buffer.Length)
    ()

[<Fact>]
let ``To Bytes (via type, null)`` () =
    let buffer = generator.ToBytes(null, typeof<Uri>)
    Assert.Equal(0, buffer.Length)
    ()

[<Theory>]
[<InlineData(3.14)>]
[<InlineData("pi")>]
let ``To Bytes ('a : obj)`` (value : obj) =
    let source = box value
    let buffer = generator.ToBytes<obj> source
    Assert.NotEmpty(buffer)
    ()

[<Fact>]
let ``To Value ('a : obj)`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.ToValue<obj> Array.empty<byte>)
    Assert.Contains("Invalid type", error.Message)
    ()

[<Theory>]
[<InlineData(2.71)>]
[<InlineData("e")>]
let ``To Value (via type)`` (value : obj) =
    let buffer = generator.ToBytes value
    let result = generator.ToValue(buffer, value.GetType())
    Assert.Equal(value, result)
    ()

[<Fact>]
let ``To Value (via type, argument null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> generator.ToValue(Array.empty<byte>, Unchecked.defaultof<Type>) |> ignore)
    Assert.Equal("type", error.ParamName)
    ()
    
[<Theory>]
[<InlineData(6)>]
[<InlineData("fox")>]
let ``To Bytes and To Value`` (data : 'a) =
    let buffer = generator.ToBytes data
    let result = generator.ToValue<'a> buffer
    Assert.Equal<'a>(data, result)
    ()

[<Fact>]
let ``Internal Types`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.ToValue<Converter> Array.empty<byte> |> ignore)
    Assert.Contains("Invalid type", error.Message)
    let error = Assert.Throws<ArgumentException>(fun () -> generator.ToValue<Generator> Array.empty<byte> |> ignore)
    Assert.Contains("Invalid type", error.Message)
    ()

// Bad creator operation
type BadType = { some : obj }

type BadConverterCreator () =
    interface IConverterCreator with
        member __.GetConverter(context, ``type``) =
            if ``type`` = typeof<BadType> then
                context.GetConverter typeof<int> // bad return value
            else null

[<Fact>]
let ``Bad Creator`` () =
    let generator = new Generator(creators = Seq.singleton(new BadConverterCreator() :> IConverterCreator))
    let error = Assert.Throws<InvalidOperationException>(fun () -> generator.GetConverter(typeof<BadType>) |> ignore)
    let message = sprintf "Invalid converter '%O', creator type: %O, expected converter item type: %O" (generator.GetConverter<int>().GetType()) typeof<BadConverterCreator> typeof<BadType>
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let generator = new Generator()
    Assert.Equal("Generator(Converters: 22, Creators: 11)", generator.ToString())
    ()
