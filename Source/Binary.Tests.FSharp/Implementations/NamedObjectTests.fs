module Implementations.NamedObjectTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let generator = Generator.CreateDefault()

[<Fact>]
let ``Key Does Not Exist`` () =
    let bytes = generator.Encode {| alpha = 2048; bravo = "Charlie" |}
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Decode(bytes, {| alpha = 0; bravo = String.Empty; delta = 0.0 |}) |> ignore)
    Assert.StartsWith("Property 'delta' does not exist, type:", error.Message)
    ()

[<Fact>]
let ``Key Already Exists`` () =
    let items = [ ("a", box 1024); ("b", box 256); ("c", box 16); ("b", box 128) ] |> List.map (fun (x, y) -> KeyValuePair(x, y))
    let bytes = generator.Encode items
    let error = Assert.Throws<ArgumentException>(fun () -> generator.Decode(bytes, {| a = 0; b = 0; c = 0 |}) |> ignore)
    Assert.StartsWith("Property 'b' already exists, type:", error.Message)
    ()

[<Fact>]
let ``Anonymous Class Record Encode (from null value)`` () =
    let template = {| id = 1024; data = "data" |}
    let converter = generator.GetConverter(template) :> IConverter
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let mutable allocator = new Allocator()
    converter.Encode(&allocator, null)
    Assert.Equal(0, allocator.Length)
    converter.EncodeWithLengthPrefix(&allocator, null)
    Assert.Equal(4, allocator.Length)
    let mutable span = ReadOnlySpan (allocator.AsSpan().ToArray())
    Assert.Equal(0, PrimitiveHelper.DecodeNumber(&span))
    ()

[<Fact>]
let ``Bytes To Anonymous Class Record (from empty bytes, expect null value)`` () =
    let converter = generator.GetConverter {| id = 0; data = String.Empty |}
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let value = converter.Decode Array.empty<byte>
    Assert.Null value
    ()

[<Fact>]
let ``Bytes To Anonymous Value Record (from empty bytes, expect bytes not enough)`` () =
    let converter = generator.GetConverter struct {| alpha = 0.0; bravo = Unchecked.defaultof<Uri> |}
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode Array.empty<byte> |> ignore)
    Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message)
    ()

type ``Item 48`` () =
    member val Item00 = "0x00" with get, set

    member val Item01 = "0x01" with get, set

    member val Item02 = "0x02" with get, set

    member val Item03 = "0x03" with get, set

    member val Item04 = "0x04" with get, set

    member val Item05 = "0x05" with get, set

    member val Item06 = "0x06" with get, set

    member val Item07 = "0x07" with get, set

    member val Item08 = "0x08" with get, set

    member val Item09 = "0x09" with get, set

    member val Item0A = "0x0A" with get, set

    member val Item0B = "0x0B" with get, set

    member val Item0C = "0x0C" with get, set

    member val Item0D = "0x0D" with get, set

    member val Item0E = "0x0E" with get, set

    member val Item0F = "0x0F" with get, set

    member val Item10 = "0x10" with get, set

    member val Item11 = "0x11" with get, set

    member val Item12 = "0x12" with get, set

    member val Item13 = "0x13" with get, set

    member val Item14 = "0x14" with get, set

    member val Item15 = "0x15" with get, set

    member val Item16 = "0x16" with get, set

    member val Item17 = "0x17" with get, set

    member val Item18 = "0x18" with get, set

    member val Item19 = "0x19" with get, set

    member val Item1A = "0x1A" with get, set

    member val Item1B = "0x1B" with get, set

    member val Item1C = "0x1C" with get, set

    member val Item1D = "0x1D" with get, set

    member val Item1E = "0x1E" with get, set

    member val Item1F = "0x1F" with get, set

    member val Item20 = "0x20" with get, set

    member val Item21 = "0x21" with get, set

    member val Item22 = "0x22" with get, set

    member val Item23 = "0x23" with get, set

    member val Item24 = "0x24" with get, set

    member val Item25 = "0x25" with get, set

    member val Item26 = "0x26" with get, set

    member val Item27 = "0x27" with get, set

    member val Item28 = "0x28" with get, set

    member val Item29 = "0x29" with get, set

    member val Item2A = "0x2A" with get, set

    member val Item2B = "0x2B" with get, set

    member val Item2C = "0x2C" with get, set

    member val Item2D = "0x2D" with get, set

    member val Item2E = "0x2E" with get, set

    member val Item2F = "0x2F" with get, set

[<Fact>]
let ``Type With 32 Properties (via constructor)`` () =
    let source = {|
        X00 = 0x00; X01 = 0x01; X02 = 0x02; X03 = 0x03; X04 = 0x04; X05 = 0x05; X06 = 0x06; X07 = 0x07;
        X08 = 0x08; X09 = 0x09; X0A = 0x0A; X0B = 0x0B; X0C = 0x0C; X0D = 0x0D; X0E = 0x0E; X0F = 0x0F;
        X10 = 0x10; X11 = 0x11; X12 = 0x12; X13 = 0x13; X14 = 0x14; X15 = 0x15; X16 = 0x16; X17 = 0x17;
        X18 = 0x18; X19 = 0x19; X1A = 0x1A; X1B = 0x1B; X1C = 0x1C; X1D = 0x1D; X1E = 0x1E; X1F = 0x1F; |}
    let converter = generator.GetConverter(source)
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let mutable allocator = new Allocator()
    converter.Encode(&allocator, source)
    let buffer = allocator.AsSpan().ToArray()
    let result = converter.Decode buffer
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal(string source, string result)
    ()

[<Fact>]
let ``Type With 48 Properties (via properties)`` () =
    let source = new ``Item 48``()
    let converter = generator.GetConverter(source)
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let mutable allocator = new Allocator()
    converter.Encode(&allocator, source)
    let buffer = allocator.AsSpan().ToArray()
    let result = converter.Decode buffer
    Assert.False(obj.ReferenceEquals(source, result))
    Assert.Equal("0x1F", result.Item1F)
    Assert.Equal("0x2F", result.Item2F)
    ()

[<Interface>]
type IPerson =
    abstract Name : string

    abstract Age : int

[<AbstractClass>]
type BasicPerson (name : string, age : int) =
    member val Name = name with get, set

    member val Age = age with get, set

[<AbstractClass>]
type AbstractPerson (name : string, age : int) =
    inherit BasicPerson(name, age)

    new (age : int, name : string) = AbstractPerson(name, age)

type Student (name : string, age : int) =
    inherit AbstractPerson(name, age)

    member __.View = sprintf "%s, %d" name age

    interface IPerson with
        member me.Name = me.Name

        member me.Age = me.Age

let test (instance : 'a) (anonymous : 'b) =
    let converter = generator.GetConverter<'a>()
    Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
    let buffer = converter.Encode instance
    let target = generator.Encode anonymous
    Assert.Equal<byte>(target, buffer)
    let error = Assert.Throws<NotSupportedException>(fun () -> converter.Decode buffer |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<'a>
    Assert.Equal(message, error.Message)
    ()

[<Fact>]
let ``No suitable constructor (interface)`` () = test (Student("Tom", 18) :> IPerson) ({| Name = "Tom"; Age = 18 |})

[<Fact>]
let ``No suitable constructor (abstract class with single pattern-constructors)`` () = test (Student("Bob", 24) :> BasicPerson) ({| Name = "Bob"; Age = 24 |})

[<Fact>]
let ``No suitable constructor (abstract class with multiple pattern-constructors)`` () = test (Student("Alice", 20) :> AbstractPerson) ({| Name = "Alice"; Age = 20 |})

[<Fact>]
let ``No suitable constructor (class with some get-only property)`` () = test (Student("Ann", 22)) ({| Name = "Ann"; Age = 22; View = "Ann, 22" |})
