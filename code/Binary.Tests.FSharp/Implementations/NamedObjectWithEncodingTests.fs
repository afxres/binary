module Implementations.NamedObjectWithEncodingTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Text
open Xunit

type EncodingStringConverter(encoding: Encoding) =
    inherit Converter<string> 0

    override __.Encode(allocator, item) =
        let bytes = encoding.GetBytes item
        Allocator.Append(&allocator, ReadOnlySpan<byte> bytes)
        ()

    override __.Decode(span: inref<ReadOnlySpan<byte>>) : string = encoding.GetString(span.ToArray())

[<Fact>]
let ``Named Object Encode (default generator)`` () =
    let generator = Generator.CreateDefault()
    let value = {| id = 2; name = "fsharp" |}
    let alpha = generator.Encode value
    Assert.Equal(8 + 12, alpha.Length)
    let dictionary = dict [ "id", box 2; "name", box "fsharp" ] |> SortedDictionary
    let bravo = generator.Encode dictionary
    Assert.Equal<byte>(alpha, bravo)
    ()

[<Fact>]
let ``Named Object Decode (default generator)`` () =
    let generator = Generator.CreateDefault()
    let dictionary = dict [ "id", box 8; "name", box "dotnet" ]
    let alpha = generator.Encode dictionary
    Assert.Equal(8 + 12, alpha.Length)
    let value = generator.Decode(alpha, anonymous = {| id = 0; name = Unchecked.defaultof<string> |})
    Assert.Equal(8, value.id)
    Assert.Equal("dotnet", value.name)
    ()

[<Fact>]
let ``Named Object As Token (default generator)`` () =
    let generator = Generator.CreateDefault()
    let dictionary = dict [ "id", box 4; "name", box "name" ]
    let alpha = generator.Encode dictionary
    Assert.Equal(8 + 10, alpha.Length)
    let token = Token(generator, ReadOnlyMemory<byte> alpha)
    Assert.Equal(2, token.Children.Count)
    Assert.Equal(4, token.["id"].As<int>())
    Assert.Equal("name", token.["name"].As<string>())
    ()

[<Fact>]
let ``Named Object Encode (utf32 string converter)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverter(EncodingStringConverter(Encoding.UTF32)).Build()
    let value = {| id = 2; name = "fsharp" |}
    let alpha = generator.Encode value
    Assert.Equal(14 + 45, alpha.Length)
    let dictionary = dict [ "id", box 2; "name", box "fsharp" ] |> SortedDictionary
    let bravo = generator.Encode dictionary
    Assert.Equal<byte>(alpha, bravo)
    ()

[<Fact>]
let ``Named Object Decode (utf32 string converter)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverter(EncodingStringConverter(Encoding.UTF32)).Build()
    let dictionary = dict [ "id", box 8; "name", box "dotnet" ]
    let alpha = generator.Encode dictionary
    Assert.Equal(14 + 45, alpha.Length)
    let value = generator.Decode(alpha, anonymous = {| id = 0; name = Unchecked.defaultof<string> |})
    Assert.Equal(8, value.id)
    Assert.Equal("dotnet", value.name)
    ()

[<Fact>]
let ``Named Object As Token (utf32 string converter)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverter(EncodingStringConverter(Encoding.UTF32)).Build()
    let dictionary = dict [ "id", box 4; "name", box "name" ]
    let alpha = generator.Encode dictionary
    Assert.Equal(14 + 34, alpha.Length)
    let token = Token(generator, ReadOnlyMemory<byte> alpha)
    Assert.Equal(2, token.Children.Count)
    Assert.Equal(4, token.["id"].As<int>())
    Assert.Equal("name", token.["name"].As<string>())
    ()

type BadStringConverter() =
    inherit Converter<string>()

    override __.Encode(_, _) =
        // do nothing (append empty string)
        ()

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : string = raise (NotSupportedException())

[<Fact>]
let ``Named Key Duplicated (bad string converter)`` () =
    let generator = Generator.CreateDefaultBuilder().AddConverter(BadStringConverter()).Build()
    let anonymous = {| alpha = 0; bravo = String.Empty |}
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(anonymous = anonymous) |> ignore)
    let message = $"Named key 'bravo' already exists, type: {anonymous.GetType()}"
    Assert.Equal(message, error.Message)
    ()
