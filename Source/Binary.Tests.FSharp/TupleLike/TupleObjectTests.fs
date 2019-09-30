module TupleLike.TupleObjectTests

open Mikodev.Binary
open Mikodev.Binary.Abstractions
open Mikodev.Binary.Attributes
open System
open Xunit

type Raw<'a> = { data : 'a }

type RawConverter<'a>(length : int) =
    inherit ConstantConverter<Raw<'a>>(length)

    override __.ToBytes(_, _) = raise (NotSupportedException())

    override __.ToValue (_ : inref<ReadOnlySpan<byte>>) : Raw<'a> = raise (NotSupportedException())

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
    let generator = new Generator(converters = [| singleConverter; doubleConverter |])
    let alpha = generator.GetConverter<Two<Raw<single>, Raw<double>>>()
    Assert.Equal(Int32.MaxValue, alpha.Length)
    ()

[<Fact>]
let ``Tuple Object Length (overflow)`` () =
    let singleConverter = RawConverter<single>(0x2000_0000)
    let doubleConverter = RawConverter<double>(0x6000_0000)
    let generator = new Generator(converters = [| singleConverter; doubleConverter |])
    let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<Two<Raw<single>, Raw<double>>>() |> ignore)
    Assert.Equal(sprintf "Converter length overflow, type: %O" typeof<Two<Raw<single>, Raw<double>>>, error.Message)
    ()
