module Attributes.InheritanceTests

open Mikodev.Binary
open Mikodev.Binary.Attributes
open System
open Xunit

let generator = Generator.CreateDefault()

type Alpha() =
    [<TupleKey(0)>]
    member val A = 0 with get, set

type Bravo() =
    inherit Alpha()

[<NamedObject>]
type Charlie() =
    inherit Alpha()

type Delta() =
    [<NamedKey("D")>]
    member val D = String.Empty with get, set

type Echo() =
    inherit Delta()

[<TupleObject>]
type Foxtrot() =
    inherit Delta()

type Romeo() =
    member val Empty = 0 with get, set

[<NamedObject>]
type Sierra() =
    inherit Romeo()

[<TupleObject>]
type Tango() =
    inherit Romeo()

[<Theory>]
[<InlineData(typeof<Bravo>, "TupleObjectAttribute", "TupleKeyAttribute", "A")>]
[<InlineData(typeof<Echo>, "NamedObjectAttribute", "NamedKeyAttribute", "D")>]
[<InlineData(typeof<Charlie>, "TupleObjectAttribute", "TupleKeyAttribute", "A")>]
[<InlineData(typeof<Foxtrot>, "NamedObjectAttribute", "NamedKeyAttribute", "D")>]
let ``Require Object Attribute For Key Attribute`` (t: Type, required : string, existed : string, propertyName : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Require '%s' for '%s', member name: %s, type: %O" required existed propertyName t
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(typeof<Sierra>, "NamedKeyAttribute", "NamedObjectAttribute")>]
[<InlineData(typeof<Tango>, "TupleKeyAttribute", "TupleObjectAttribute")>]
let ``Require Key Attribute For Object Attribute`` (t: Type, required : string, existed : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Require '%s' for '%s', type: %O" required existed t
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
let ``Multiple Attributes On Base Class Property`` (t : Type, propertyName : string) =
    let thisError = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t |> ignore)
    let baseError = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter t.BaseType |> ignore)
    let thisMessage = sprintf "Multiple attributes found, member name: %s, type: %O" propertyName t
    let baseMessage = sprintf "Multiple attributes found, member name: %s, type: %O" propertyName t.BaseType
    Assert.Equal(thisMessage, thisError.Message)
    Assert.Equal(baseMessage, baseError.Message)
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
[<InlineData(typeof<Juliet>, 0)>]
let ``Tuple Key Duplicated`` (t : Type, key : int) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Tuple key '%d' already exists, type: %O" key t
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
[<InlineData(typeof<Lima>, "item")>]
let ``Named Key Duplicated`` (t : Type, key : string) =
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Named key '%s' already exists, type: %O" key t
    Assert.Equal(message, error.Message)
    ()

[<NamedObject>]
type Mike () =
    [<NamedKey("M")>]
    member val M = 3L with get, set

type November () =
    class
    inherit Mike()
    end

[<Theory>]
[<InlineData(typeof<November>, "NamedObjectAttribute", "NamedKeyAttribute", "M")>]
let ``Require Object Attribute On This Class For Base Class Key Attribute`` (t: Type, required : string, existed : string, propertyName : string) =
    // ensure base class works
    generator.GetConverter t.BaseType |> ignore
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Require '%s' for '%s', member name: %s, type: %O" required existed propertyName t
    Assert.Equal(message, error.Message)
    ()

[<TupleObject>]
type Oscar () =
    abstract member O : int with get, set

    [<TupleKey(0)>]
    default val O = 0 with get, set

    override me.Equals obj = match obj with | :? Oscar as a -> a.O = me.O | _ -> false

    override me.GetHashCode () = me.O.GetHashCode()

[<TupleObject>]
type Papa () =
    inherit Oscar()

    override val O = 1 with get, set

[<NamedObject>]
type Quebec () =
    inherit Oscar()

    [<NamedKey("Oscar")>]
    override val O = 2 with get, set

[<Theory>]
[<InlineData(typeof<Papa>, "TupleKeyAttribute", "TupleObjectAttribute")>]
let ``Require Key Attribute On This Type For Object Attribute`` (t: Type, required : string, existed : string) =
    // ensure base class works
    generator.GetConverter t.BaseType |> ignore
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
    let message = sprintf "Require '%s' for '%s', type: %O" required existed t
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(typeof<Quebec>, "NamedObjectConverter`1", "TupleObjectConverter`1")>]
let ``Rewrite Base Class Attribute On This Class`` (t : Type, thisConverterDefinition : string, baseConverterDefinition : string) =
    let thisConverter = generator.GetConverter t
    let baseConverter = generator.GetConverter t.BaseType
    let thisDefinition = thisConverter.GetType().GetGenericTypeDefinition()
    let baseDefinition = baseConverter.GetType().GetGenericTypeDefinition()
    Assert.NotEqual(thisDefinition, baseDefinition)
    Assert.Equal(thisConverterDefinition, thisDefinition.Name)
    Assert.Equal(baseConverterDefinition, baseDefinition.Name)

    let object = Activator.CreateInstance t
    let thisBuffer = thisConverter.Encode object
    let baseBuffer = baseConverter.Encode object
    let thisResult = thisConverter.Decode thisBuffer
    let baseResult = baseConverter.Decode baseBuffer
    Assert.Equal(object, thisResult)
    Assert.Equal(object, baseResult)
    ()
