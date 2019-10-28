namespace Classes

open Mikodev.Binary
open System
open Xunit

type FakeConverter<'a>() =
    inherit Converter<'a>()

    override __.Encode(_, _) = raise (NotSupportedException("Text alpha"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'a = raise (NotSupportedException("Text bravo"))

    override __.Encode(_) = raise (NotSupportedException("Text charlie"))

    override __.Decode(_ : byte array) : 'a = raise (NotSupportedException("Text delta"))

type GeneratorExtensionsTests() =
    let generator = Generator.CreateDefault()

    let GeneratorBuilder() =
        let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GeneratorBuilder") |> Array.exactlyOne
        let builder = Activator.CreateInstance(t)
        builder :?> IGeneratorBuilder

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| 256 |]
            yield [| 2.0 |]
            yield [| struct (2L, 'Z') |]
            yield [| ("001", 20, struct (true, Guid.NewGuid())) |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode No Generic`` (value : obj) =
        let buffer = generator.Encode(value, value.GetType())
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode(&memory, value.GetType())
        let bravo = generator.Decode(buffer, value.GetType())
        Assert.Equal(value, alpha)
        Assert.Equal(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode`` (value : 'A) =
        let buffer = generator.Encode value
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode<'A> &memory
        let bravo = generator.Decode<'A> buffer
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Decode With Anonymous`` (value : 'A) =
        let buffer = generator.Encode value
        let memory = ReadOnlySpan buffer

        let alpha = generator.Decode(&memory, anonymous = value)
        let bravo = generator.Decode(buffer, anonymous = value)
        Assert.Equal<'A>(value, alpha)
        Assert.Equal<'A>(value, bravo)
        ()

    [<Fact>]
    member __.``Route Encode`` () =
        let generator = GeneratorBuilder().AddConverter(FakeConverter<int>()).Build()
        let error = Assert.Throws<NotSupportedException>(fun () -> generator.Encode(0) |> ignore)
        Assert.Equal("Text charlie", error.Message)
        ()

    [<Fact>]
    member __.``Route Encode Non Generic`` () =
        let generator = GeneratorBuilder().AddConverter(FakeConverter<string>()).Build()
        let error = Assert.Throws<NotSupportedException>(fun () -> generator.Encode(null, typeof<string>) |> ignore)
        Assert.Equal("Text charlie", error.Message)
        ()

    [<Fact>]
    member __.``Route Decode Span`` () =
        let generator = GeneratorBuilder().AddConverter(FakeConverter<string>()).Build()
        let a = Assert.Throws<NotSupportedException>(fun () -> let span = ReadOnlySpan<byte>() in generator.Decode(&span, typeof<string>) |> ignore)
        let b = Assert.Throws<NotSupportedException>(fun () -> let span = ReadOnlySpan<byte>() in generator.Decode<string>(&span) |> ignore)
        let c = Assert.Throws<NotSupportedException>(fun () -> let span = ReadOnlySpan<byte>() in generator.Decode(&span, "anonymous") |> ignore)
        let message = "Text bravo"
        Assert.Equal(message, a.Message)
        Assert.Equal(message, b.Message)
        Assert.Equal(message, c.Message)
        ()

    [<Fact>]
    member __.``Route Decode Byte Array`` () =
        let generator = GeneratorBuilder().AddConverter(FakeConverter<string>()).Build()
        let buffer = Array.empty<byte>
        let a = Assert.Throws<NotSupportedException>(fun () -> generator.Decode(buffer, typeof<string>) |> ignore)
        let b = Assert.Throws<NotSupportedException>(fun () -> generator.Decode<string>(buffer) |> ignore)
        let c = Assert.Throws<NotSupportedException>(fun () -> generator.Decode(buffer, "anonymous") |> ignore)
        let message = "Text delta"
        Assert.Equal(message, a.Message)
        Assert.Equal(message, b.Message)
        Assert.Equal(message, c.Message)
        ()

    static member ``Data Bravo`` : (obj array) seq = seq {
        yield [| typeof<int> |]
        yield [| typeof<string> |]
        yield [| typeof<Nullable<double>> |]
    }

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Invalid System Type`` (t : Type) =
        let generator = GeneratorBuilder().Build()
        Assert.Equal("Generator(Converters: 1, Creators: 0)", generator.ToString())
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
        let message = sprintf "Invalid system type: %O" t
        Assert.Equal(message, error.Message)
        ()
