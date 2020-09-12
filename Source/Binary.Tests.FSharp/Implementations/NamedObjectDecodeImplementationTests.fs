module Implementations.NamedObjectDecodeImplementationTests

open Mikodev.Binary
open System
open Xunit

[<Class>]
type Alpha(a, b, c) =
    member __.A : int = a

    member __.B : Guid = b

    member __.C : string = c

[<Struct>]
type Bravo(b : string, c : byte, a : float) =
    member __.B : string = b

    member __.C : byte = c

    member __.A : float = a

[<Class>]
type Charlie () =
    let mutable a : string = null

    let mutable b : int = 0

    let mutable c : float = 0.0

    member __.A with get () = a and set value = a <- value

    member __.B with get () = b and set value = b <- value

    member __.C with get () = c and set value = c <- value

[<Struct>]
type Delta =
    val mutable private c : string

    val mutable private a : byte

    val mutable private b : int

    member this.C with get () = this.c and set value = this.c <- value

    member this.A with get () = this.a and set value = this.a <- value

    member this.B with get () = this.b and set value = this.b <- value

let generator = Generator.CreateDefault()

[<Fact>]
let ``Class Via Constructor`` () =
    let a = new Alpha(3, Guid.NewGuid(), "three")
    let bytes = generator.Encode a
    let value = generator.Decode<Alpha> bytes
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

[<Fact>]
let ``Struct Via Constructor`` () =
    let a = new Bravo("four", byte 4, 4.4)
    let bytes = generator.Encode a
    let value = generator.Decode<Bravo> bytes
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

[<Fact>]
let ``Class Via Properties`` () =
    let a = new Charlie()
    a.A <- "charlie"; a.B <- -3; a.C <- -2.2
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let value = generator.Decode<Charlie> bytes
    Assert.False(obj.ReferenceEquals(a, value))
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

[<Fact>]
let ``Struct Via Properties`` () =
    let mutable a = new Delta()
    a.A <- byte -2; a.B <- -3; a.C <- "zero"
    let bytes = generator.Encode a
    Assert.NotEmpty bytes
    let value = generator.Decode<Delta> bytes
    Assert.False(obj.ReferenceEquals(a, value))
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

type AlphaUnordered(first : int, second : string, last : double) =
    member __.Second = second

    member __.First = first

    member __.Last = last

type AlphaMultipleConstructors(a : int, bravo : string, charlie : Guid) =
    member __.A = a

    member __.Bravo = bravo

    member __.Charlie = charlie

    new (charlie : Guid, a : int, bravo : string) = new AlphaMultipleConstructors(a, bravo, charlie)

type NamedTypeMismatch(head : int, body : string) =
    member __.Head = head.ToString()

    member __.Body = let _, data = Int32.TryParse(body) in data

[<Fact>]
let ``Class Via Constructor Ordered`` () =
    let constructor = typeof<AlphaUnordered>.GetConstructors() |> Array.exactlyOne
    let parameters = constructor.GetParameters()
    let properties = typeof<AlphaUnordered>.GetProperties()
    let parameterNames = parameters |> Array.map (fun x -> x.Name.ToUpperInvariant())
    let propertyNames = properties |> Array.map (fun x -> x.Name.ToUpperInvariant())
    Assert.NotEqual<string>(parameterNames, propertyNames)
    Assert.Equal<string>(parameterNames |> Set.ofArray, propertyNames |> Set.ofArray)
    let source = new AlphaUnordered(100, "data source", 2.718)
    let buffer = generator.Encode source
    let result = generator.Decode<AlphaUnordered> buffer
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(source.First, result.First)
    Assert.Equal(source.Second, result.Second)
    Assert.Equal(source.Last, result.Last)
    ()

[<Fact>]
let ``Class Via Constructor With Multiple Suitable Constructors`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<AlphaMultipleConstructors>() |> ignore)
    Assert.Equal(sprintf "Multiple suitable constructors found, type: %O" typeof<AlphaMultipleConstructors>, error.Message)
    ()

[<Fact>]
let ``Class Via Constructor With Type Mismatch Properties`` () =
    let converter = generator.GetConverter<NamedTypeMismatch>()
    let buffer = converter.Encode (NamedTypeMismatch(1024, "4096"))
    let expect = generator.Encode {| Head = "1024"; Body = 4096 |}
    Assert.Equal<byte>(expect, buffer)
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode buffer |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<NamedTypeMismatch>
    Assert.Equal(message, error.Message)
    ()
