module Implementations.TupleObjectTests

open Mikodev.Binary
open Mikodev.Binary.Attributes
open System
open Xunit

let generator = Generator.CreateDefault()

type Raw<'a> = { data : 'a }

type RawConverter<'a>(length : int) =
    inherit Converter<Raw<'a>>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : Raw<'a> = raise (NotSupportedException())

[<TupleObject>]
type Two<'a, 'b>(a : 'a, b : 'b) =
    [<TupleKey(0)>]
    member __.A = a

    [<TupleKey(1)>]
    member __.B = b

[<Fact>]
let ``Tuple Object Length (max value)`` () =
    let singleConverter = RawConverter<single>(0x3000_0000)
    let doubleConverter = RawConverter<double>(0x4FFF_FFFF)
    //let generator = new Generator(converters = [| singleConverter; doubleConverter |])
    let generator = Generator.CreateDefaultBuilder()
                        .AddConverter(singleConverter)
                        .AddConverter(doubleConverter)
                        .Build();
    let alpha = generator.GetConverter<Two<Raw<single>, Raw<double>>>()
    Assert.Equal(Int32.MaxValue, alpha.Length)
    ()

[<Fact>]
let ``Tuple Object Length (overflow)`` () =
    let singleConverter = RawConverter<single>(0x2000_0000)
    let doubleConverter = RawConverter<double>(0x6000_0000)
    let generator = Generator.CreateDefaultBuilder()
                        .AddConverter(singleConverter)
                        .AddConverter(doubleConverter)
                        .Build();
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<Two<Raw<single>, Raw<double>>>() |> ignore)
    Assert.Equal(sprintf "Converter length overflow, type: %O" typeof<Two<Raw<single>, Raw<double>>>, error.Message)
    ()

[<Interface>]
[<TupleObject>]
type ICar =
    [<TupleKey(0)>]
    abstract Name : string

    [<TupleKey(1)>]
    abstract Rank : int

[<AbstractClass>]
[<TupleObject>]
type BasicCar(name : string, rank : int) =
    [<TupleKey(1)>]
    member val Name = name with get, set

    [<TupleKey(0)>]
    member val Rank = rank with get, set

[<AbstractClass>]
[<TupleObject>]
type AbstractCar(name : string, rank : int) =
    inherit BasicCar(name, rank)

    new (rank : int, name : string) = AbstractCar(name, rank)

[<TupleObject>]
type Car(name : string, rank : int) =
    inherit AbstractCar(name, rank)

    [<TupleKey(2)>]
    member __.Data = sprintf "%s - %d" name rank

    interface ICar with
        member me.Name = me.Name

        member me.Rank = me.Rank

let test (instance : 'a) (anonymous : 'b) =
    let converter = generator.GetConverter<'a>()
    Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name)
    let buffer = converter.Encode instance
    let target = generator.Encode anonymous
    Assert.Equal<byte>(target, buffer)
    let alpha = Assert.Throws<NotSupportedException>(fun () -> converter.Decode buffer |> ignore)
    let bravo = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan buffer in converter.DecodeAuto &span |> ignore)
    let message = sprintf "No suitable constructor found, type: %O" typeof<'a>
    Assert.Equal(message, alpha.Message)
    Assert.Equal(message, bravo.Message)
    ()

[<Fact>]
let ``No suitable constructor (interface)`` () = test (Car("Mini", 90) :> ICar) ("Mini", 90)

[<Fact>]
let ``No suitable constructor (abstract class with single pattern-constructors)`` () = test (Car("Horse", 95) :> BasicCar) (95, "Horse")

[<Fact>]
let ``No suitable constructor (abstract class with multiple pattern-constructors)`` () = test (Car("16", 90) :> AbstractCar) (90, "16")

[<Fact>]
let ``No suitable constructor (class with some get-only property)`` () = test (Car("Toy", 70)) (70, "Toy", "Toy - 70")
