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

    member me.C with get () = me.c and set value = me.c <- value

    member me.A with get () = me.a and set value = me.a <- value

    member me.B with get () = me.b and set value = me.b <- value

let generator = Generator.CreateDefault()

[<Fact>]
let ``Class Via Constructor`` () =
    let a = Alpha(3, Guid.NewGuid(), "three")
    let bytes = generator.Encode a
    let value = generator.Decode<Alpha> bytes
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

[<Fact>]
let ``Struct Via Constructor`` () =
    let a = Bravo("four", byte 4, 4.4)
    let bytes = generator.Encode a
    let value = generator.Decode<Bravo> bytes
    Assert.Equal(a.A, value.A)
    Assert.Equal(a.B, value.B)
    Assert.Equal(a.C, value.C)
    ()

[<Fact>]
let ``Class Via Properties`` () =
    let a = Charlie()
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
    let mutable a = Delta()
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

    new (charlie : Guid, a : int, bravo : string) = AlphaMultipleConstructors(a, bravo, charlie)

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
    let source = AlphaUnordered(100, "data source", 2.718)
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
    let message = sprintf "Multiple suitable constructors found, type: %O" typeof<AlphaMultipleConstructors>
    Assert.Equal(message, error.Message)
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

type ClassViaConstructorThenMembers(three : string, one : int) =
    member val Two = 0.0 with get, set

    member __.One = one

    member __.Three = three

[<Fact>]
let ``Class Via Constructor Then Members`` () =
    let source = ClassViaConstructorThenMembers("23", 21, Two = 22.2)
    let converter = generator.GetConverter(anonymous = source)
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal(source.One, result.One)
    Assert.Equal(source.Two, result.Two)
    Assert.Equal(source.Three, result.Three)
    ()

type ClassMultipleConstructorThenMembers private(first : single, second : double, third : string, fourth : Guid) =
    member __.First = first

    member val Second = second with get, set

    member val Third = third with get, set

    member val Fourth = fourth with get, set

    new (first) = ClassMultipleConstructorThenMembers(first, 0.0, String.Empty, Guid())

    new (first, second) = ClassMultipleConstructorThenMembers(first, second, String.Empty, Guid())

    new (first, second, third) = ClassMultipleConstructorThenMembers(first, second, third, Guid())

[<Fact>]
let ``Class Via Constructor Then Members With Multiple Suitable Constructors`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<ClassMultipleConstructorThenMembers>() |> ignore)
    let message = sprintf "Multiple suitable constructors found, type: %O" typeof<ClassMultipleConstructorThenMembers>
    Assert.Equal(message, error.Message)
    ()

type ClassSingleFullConstructorMultipleConstructorThenMembers(first : single, second : double, third : string, fourth : Guid) =
    member __.First = first

    member val Second = second with get, set

    member val Third = third with get, set

    member val Fourth = fourth with get, set

    new (first) = ClassSingleFullConstructorMultipleConstructorThenMembers(first, 0.0, String.Empty, Guid())

    new (first, second) = ClassSingleFullConstructorMultipleConstructorThenMembers(first, second, String.Empty, Guid())

    new (first, second, third) = ClassSingleFullConstructorMultipleConstructorThenMembers(first, second, third, Guid())

[<Fact>]
let ``Class Via Constructor Or Constructor Then Members Valid Via Constructor`` () =
    let source = ClassSingleFullConstructorMultipleConstructorThenMembers(single 1.1, 2.2, "3.3", Guid.NewGuid())
    let converter = generator.GetConverter(anonymous = source)
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal(source.First, result.First)
    Assert.Equal(source.Second, result.Second)
    Assert.Equal(source.Third, result.Third)
    Assert.Equal(source.Fourth, result.Fourth)
    ()

type ClassMultipleFullConstructorMultipleConstructorThenMembers(first : single, second : double, third : string, fourth : Guid) =
    member __.First = first

    member val Second = second with get, set

    member val Third = third with get, set

    member val Fourth = fourth with get, set

    new (first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembers(first, 0.0, String.Empty, Guid())

    new (first : single, second) = ClassMultipleFullConstructorMultipleConstructorThenMembers(first, second, String.Empty, Guid())

    new (first : single, second, third) = ClassMultipleFullConstructorMultipleConstructorThenMembers(first, second, third, Guid())

    new (fourth : Guid, third : string, second : double, first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembers(first, second, third, fourth)

    new (fourth : Guid, second : double, third : string, first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembers(first, second, third, fourth)

[<Fact>]
let ``Class Via Constructor Or Constructor Then Members All Invalid`` () =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<ClassMultipleFullConstructorMultipleConstructorThenMembers>() |> ignore)
    let message = sprintf "Multiple suitable constructors found, type: %O" typeof<ClassMultipleFullConstructorMultipleConstructorThenMembers>
    Assert.Equal(message, error.Message)
    ()

type ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first : single, second : double, third : string, fourth : Guid) =
    member val First = first with get, set

    member val Second = second with get, set

    member val Third = third with get, set

    member val Fourth = fourth with get, set

    new () = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(single 0, 0.0, String.Empty, Guid())

    new (first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first, 0.0, String.Empty, Guid())

    new (first : single, second) = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first, second, String.Empty, Guid())

    new (first : single, second, third) = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first, second, third, Guid())

    new (fourth : Guid, third : string, second : double, first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first, second, third, fourth)

    new (fourth : Guid, second : double, third : string, first : single) = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(first, second, third, fourth)

[<Fact>]
let ``Class Via Members Or Constructor Or Constructor Then Members Valid Via Members`` () =
    let source = ClassMultipleFullConstructorMultipleConstructorThenMembersValidViaMembers(single 3.1, 3.2, "3.3", Guid.NewGuid())
    let converter = generator.GetConverter(anonymous = source)
    let buffer = converter.Encode source
    let result = converter.Decode buffer
    Assert.Equal(source.First, result.First)
    Assert.Equal(source.Second, result.Second)
    Assert.Equal(source.Third, result.Third)
    Assert.Equal(source.Fourth, result.Fourth)
    ()
