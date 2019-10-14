module Attributes.InheritanceTests

open Mikodev.Binary
open Mikodev.Binary.Attributes
open System
open Xunit

let generator = Generator()

type Alpha () =
    [<TupleKey(0)>]
    member val A = 0 with get, set

type Bravo () =
    inherit Alpha()

[<NamedObject>]
type Charlie () =
    inherit Alpha()

type Delta () =
    [<NamedKey("D")>]
    member val D = String.Empty with get, set

type Echo () =
    inherit Delta()

[<TupleObject>]
type Foxtrot () =
    inherit Delta()

[<Theory>]
[<InlineData(typeof<Bravo>, "TupleObjectAttribute", "TupleKeyAttribute", "A")>]
[<InlineData(typeof<Charlie>, "NamedKeyAttribute", "NamedObjectAttribute", "A")>]
[<InlineData(typeof<Echo>, "NamedObjectAttribute", "NamedKeyAttribute", "D")>]
[<InlineData(typeof<Foxtrot>, "TupleKeyAttribute", "TupleObjectAttribute", "D")>]
let ``Require Attribute`` (t: Type, required : string, existed : string, propertyName : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Require '%s' for '%s', property name: %s, type: %O" required existed propertyName t
    Assert.Equal(message, error.Message)
    ()

type Golf () =
    [<TupleKey(0)>]
    [<NamedKey("G")>]
    member val G = Guid.Empty with get, set

type Hotel () =
    inherit Golf()

[<Theory>]
[<InlineData(typeof<Hotel>, "G")>]
let ``Multiple Attributes On Property`` (t : Type, propertyName : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Multiple attributes found, property name: %s, type: %O" propertyName t
    Assert.Equal(message, error.Message)
    ()

type India () =
    [<TupleKey(0)>]
    member val I = 0 with get, set

[<TupleObject>]
type Juliet () =
    inherit India()

    [<TupleKey(0)>]
    member val A = 0 with get, set

[<Theory>]
[<InlineData(typeof<Juliet>, "I", 0)>]
let ``Tuple Key Duplicated`` (t : Type, propertyName : string, key : int) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Tuple key '%d' already exists, property name: %s, type: %O" key propertyName t
    Assert.Equal(message, error.Message)
    ()

type Kilo () =
    [<NamedKey("item")>]
    member val K = String.Empty with get, set

[<NamedObject>]
type Lima () =
    inherit Kilo()

    [<NamedKey("item")>]
    member val A = 0 with get, set

[<Theory>]
[<InlineData(typeof<Lima>, "K", "item")>]
let ``Named Key Duplicated`` (t : Type, propertyName : string, key : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Named key '%s' already exists, property name: %s, type: %O" key propertyName t
    Assert.Equal(message, error.Message)
    ()
