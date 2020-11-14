module Contexts.AllocatorStringTests

open Mikodev.Binary
open System
open System.Text
open Xunit

let random = Random()

let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.IsValueType && x.Name = "Allocator") |> Array.exactlyOne

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, 世界")>]
[<InlineData("今日はいい天気ですね")>]
let ``Append String (utf8)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    Allocator.Append(&allocator, span, Encoding.UTF8)
    let buffer = allocator.AsSpan().ToArray()
    let target = Encoding.UTF8.GetBytes text
    Assert.Equal<byte>(buffer, target)

    let span = allocator.AsSpan()
    let result = Encoding.UTF8.GetString span
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("one two three four five")>]
[<InlineData("今晚打老虎")>]
let ``Append String (unicode)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    Allocator.Append(&allocator, span, Encoding.Unicode)
    let buffer = allocator.AsSpan().ToArray()
    let target = Encoding.Unicode.GetBytes text
    Assert.Equal<byte>(buffer, target)

    let span = allocator.AsSpan()
    let result = Encoding.Unicode.GetString span
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, world!")>]
[<InlineData("你好, 世界!")>]
let ``Append String With Length Prefix (utf8)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    Allocator.AppendWithLengthPrefix(&allocator, span, Encoding.UTF8)
    let mutable span = allocator.AsSpan()
    let target = Encoding.UTF8.GetBytes text
    let length = Converter.Decode &span
    Assert.Equal(target.Length, length)
    Assert.Equal(target.Length, span.Length)
    Assert.Equal<byte>(target, span.ToArray())

    let mutable span = allocator.AsSpan()
    let result = Encoding.UTF8.GetString(Converter.DecodeWithLengthPrefix &span)
    Assert.True(span.IsEmpty)
    Assert.Equal(text, result)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, world!")>]
[<InlineData("你好, 世界!")>]
let ``Append String With Length Prefix (unicode)`` (text : string) =
    let mutable allocator = Allocator()
    let span = text.AsSpan()
    Allocator.AppendWithLengthPrefix(&allocator, span, Encoding.Unicode)
    let mutable span = allocator.AsSpan()
    let target = Encoding.Unicode.GetBytes text
    let length = Converter.Decode &span
    Assert.Equal(target.Length, length)
    Assert.Equal(target.Length, span.Length)
    Assert.Equal<byte>(target, span.ToArray())

    let mutable span = allocator.AsSpan()
    let result = Encoding.Unicode.GetString(Converter.DecodeWithLengthPrefix &span)
    Assert.True(span.IsEmpty)
    Assert.Equal(text, result)
    ()

[<Fact>]
let ``Append String (null, utf8)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    Allocator.Append(&allocator, span, Encoding.UTF8);
    Assert.Equal(0, allocator.Length)
    ()

[<Fact>]
let ``Append String (null, unicode)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    Allocator.Append(&allocator, span, Encoding.Unicode);
    Assert.Equal(0, allocator.Length)
    ()

[<Fact>]
let ``Append String With Length Prefix (null, utf8)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    Allocator.AppendWithLengthPrefix(&allocator, span, Encoding.UTF8);
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal(byte 0, Assert.Single(buffer))
    ()

[<Fact>]
let ``Append String With Length Prefix (null, unicode)`` () =
    let mutable allocator = Allocator()
    let text = Unchecked.defaultof<string>
    Assert.Null text
    let span = text.AsSpan()
    Allocator.AppendWithLengthPrefix(&allocator, span, Encoding.Unicode);
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal(byte 0, Assert.Single(buffer))
    ()

[<Fact>]
let ``Append String (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        let mutable allocator = Allocator()
        let span = text.AsSpan()
        Allocator.Append(&allocator, span, encoding)
        let buffer = allocator.AsSpan().ToArray()
        let result = encoding.GetString buffer
        Assert.Equal(text, result)
    ()

[<Fact>]
let ``Append String With Length Prefix (random, from 0 to 1024)`` () =
    let encoding = Encoding.UTF8

    for i = 0 to 1024 do
        let data = [| for k = 0 to (i - 1) do yield char (random.Next(32, 127)) |]
        let text = String data
        Assert.Equal(i, text.Length)

        let mutable allocator = Allocator()
        let span = text.AsSpan()
        Allocator.AppendWithLengthPrefix(&allocator, span, encoding)
        let buffer = allocator.AsSpan().ToArray()
        let mutable span = ReadOnlySpan buffer
        let length = Converter.Decode &span
        let result = encoding.GetString (span.ToArray())
        let prefixLength = buffer.Length - length
        Assert.True(prefixLength > 0)
        Assert.Equal(i, span.Length)
        Assert.Equal(i, length)
        Assert.Equal(text, result)
    ()

[<Fact>]
let ``Append String (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        let span = String.Empty.AsSpan()
        Allocator.Append(&allocator, span, null)
        ())
    let method = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && (x.GetParameters() |> Array.last).ParameterType = typeof<Encoding>) |> Array.exactlyOne
    let parameter = method.GetParameters() |> Array.last
    Assert.Equal("encoding", parameter.Name)
    Assert.Equal("encoding", error.ParamName)
    ()

[<Fact>]
let ``Append String With Length Prefix (encoding null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        let span = String.Empty.AsSpan()
        Allocator.AppendWithLengthPrefix(&allocator, span, null)
        ())
    let method = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "AppendWithLengthPrefix" && (x.GetParameters() |> Array.last).ParameterType = typeof<Encoding>) |> Array.exactlyOne
    let parameter = method.GetParameters() |> Array.last
    Assert.Equal("encoding", parameter.Name)
    Assert.Equal("encoding", error.ParamName)
    ()
