module TupleLike.TupleTests

open Mikodev.Binary
open System
open System.Net
open Xunit

let generator =
    Generator.CreateDefaultBuilder()
        .AddFSharpConverterCreators()
        .Build();

let Test (ls : int) (ll : int) (value : 'T) =
    let c = generator.GetConverter<'T> ()
    let mutable allocator = Allocator()
    c.Encode(&allocator, value)
    let ba = allocator.AsSpan().ToArray()
    let ra = c.Decode ba

    let mutable allocator = Allocator()
    c.EncodeAuto(&allocator, value)
    let bb = allocator.AsSpan().ToArray()
    let mutable span = ReadOnlySpan<byte>(bb)
    let rb = c.DecodeAuto(&span)

    Assert.Equal<'T>(value, ra)
    Assert.Equal<'T>(value, rb)
    Assert.Equal(ls, Array.length ba)
    Assert.Equal(ll, Array.length bb)
    ()

let TestNull<'T> () =
    let value = Unchecked.defaultof<'T>
    let converter = generator.GetConverter<'T> ()
    let message = sprintf "Tuple can not be null, type: %O" typeof<'T>
    let alpha = Assert.Throws<ArgumentException>(fun () -> let mutable allocator = Allocator() in converter.Encode(&allocator, value))
    Assert.Null(alpha.ParamName)
    Assert.StartsWith(message, alpha.Message)
    let bravo = Assert.Throws<ArgumentException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, value))
    Assert.Null(bravo.ParamName)
    Assert.StartsWith(message, bravo.Message)
    ()

[<Fact>]
let ``Tuple Null 1`` () =
    TestNull<Tuple<int>> ()
    ()

[<Fact>]
let ``Tuple Null 2`` () =
    TestNull<string * string> ()
    ()

[<Fact>]
let ``Tuple Null 3`` () =
    TestNull<int * double * string> ()
    ()

[<Fact>]
let ``Tuple Null 4`` () =
    TestNull<int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 5`` () =
    TestNull<int16 * int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 6`` () =
    TestNull<byte * int16 * int * double * Guid * string> ()
    ()

[<Fact>]
let ``Tuple Null 7`` () =
    TestNull<byte * int16 * int * double * Guid * string * IPAddress> ()
    ()

[<Fact>]
let ``Tuple Null 8`` () =
    TestNull<byte * int16 * int * double * Guid * string * IPAddress * IPEndPoint> ()
    ()

[<Fact>]
let ``Value Tuple Empty Bytes`` () =
    Assert.Throws<ArgumentException>(fun () -> generator.Decode<struct (int * int)> Array.empty |> ignore) |> ignore
    ()

[<Fact>]
let ``Tuple Array`` () =
    [ 1, "one"; 2, "two"; 3, "three" ] |> Test 26 30
    [ struct (4, "four"); struct (5, "five") ] |> Test 18 22
    ()

[<Fact>]
let ``Tuple 0`` () =
    let alpha = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<Tuple>() |> ignore)
    let bravo = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<ValueTuple>() |> ignore)
    Assert.Equal(sprintf "Invalid static type: %O" typeof<Tuple>, alpha.Message)
    Assert.Equal(sprintf "Invalid type: %O" typeof<ValueTuple>, bravo.Message)
    ()

[<Fact>]
let ``Tuple 1`` () =
    Tuple.Create<int> 1 |> Test 4 4
    ValueTuple.Create 2.0 |> Test 8 8
    Tuple.Create<string> "three" |> Test 5 6
    ValueTuple.Create<string> "four" |> Test 4 5
    ()

[<Fact>]
let ``Tuple 2`` () =
    (5, 6) |> Test 8 8
    (struct (7, 8)) |> Test 8 8
    (9, "zero") |> Test 8 9
    (struct ("ten", 11)) |> Test 8 8
    ()

[<Fact>]
let ``Tuple 3`` () =
    (1, 3, 5) |> Test 12 12
    (struct (2.2, 4.8, 6.12)) |> Test 24 24
    ("a", 22, "ccc") |> Test 9 10
    (struct (4, "ee", 666)) |> Test 11 11
    ()

[<Fact>]
let ``Tuple 4`` () =
    (9, 8, 7, 6) |> Test 16 16
    (struct (5, 4, 3, 2)) |> Test 16 16
    ("x", -1, "z", -2) |> Test 12 12
    (struct (-2, "y", 3, "w")) |> Test 11 12
    ()

[<Fact>]
let ``Tuple 5`` () =
    (2, 4, 6, 8, 0) |> Test 20 20
    (struct (9, 7, 5, 3, 1)) |> Test 20 20
    ("x", "y", "z", "w", -1) |> Test 12 12
    (struct (16, "i", "j", "m", "n")) |> Test 11 12
    ()

[<Fact>]
let ``Tuple 6`` () =
    (1, 2, 4, 8, 16, 32) |> Test 24 24
    (struct (0, 1, 3, 7, 15, 31)) |> Test 24 24
    ("t", "U", "p", "L", "e", 6) |> Test 14 14
    (struct (8, "s", "h", "a", "r", "p")) |> Test 13 14
    ()

[<Fact>]
let ``Tuple 7`` () =
    (9, 11, 13, 15, 17, 19, 21) |> Test 28 28
    (struct (-2, 3, -5, 7, -11, 13, -17)) |> Test 28 28
    ("alpha", 1024, "bravo", -65536, "charlie", 33, "delta") |> Test 37 38
    (struct (7, "echo", "foxtrot", "golf", "hotel", 17, 19)) |> Test 36 36
    ()

[<Fact>]
let ``Tuple 8`` () =
    (3, 4, 5, 6, 7, 8, 9, 0) |> Test 32 32
    (struct (8, 7, 6, 5, 4, 3, 2, 1)) |> Test 32 32
    ("v", 8, "a", 6, "l", 4, "u", "e") |> Test 21 22
    (struct (-3, 6, -9, 12, -15, "no", "yes", "fine")) |> Test 31 32
    ()

[<Fact>]
let ``Tuple N`` () =
    let a = (0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 9, 8)
    let b = struct ("ok", 25, "yes", "none", 2.0, "e", "pi", "data", 3.0F, 9y)
    let typeA = a.GetType()
    let typeB = b.GetType()
    Assert.Equal("Tuple`8", typeA.Name)
    Assert.Equal("ValueTuple`8", typeB.Name)
    Assert.True(typeA.GetProperties() |> Seq.exists (fun x -> x.Name = "Rest"))
    Assert.True(typeB.GetFields() |> Seq.exists (fun x -> x.Name = "Rest"))
    Test 48 48 a
    Test 39 39 b
    ()

let TestSize<'T> size =
    let converter = generator.GetConverter<'T> ()
    Assert.Equal(size, converter.Length)
    ()

[<Fact>]
let ``Tuple Length`` () =
    TestSize<int * int64> 12
    TestSize<Tuple<string>> 0
    TestSize<ValueTuple<string>> 0
    TestSize<ValueTuple<int>> 4
    TestSize<int * string> 0
    TestSize<int * double * Guid> 28
    TestSize<byte * int16 * int32 * uint64> 15
    TestSize<int * int * int * int * int * int * int * int * int> 36
    ()

type Fix = { some : obj }

type FixConverter(length : int) =
    inherit Converter<Fix>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : Fix = raise (NotSupportedException())

[<Fact>]
let ``Tuple Length (overflow)`` () =
    let fixConverter = FixConverter(0x2000_0000) :> IConverter
    let fixGenerator = Generator.CreateDefaultBuilder().AddConverter(fixConverter).Build()
    let alpha = fixGenerator.GetConverter<Fix * Fix>()
    let bravo = fixGenerator.GetConverter<struct (Fix * Fix * Fix)>()
    Assert.Throws<OverflowException>(fun () -> fixGenerator.GetConverter<Fix * Fix * Fix * Fix>() |> ignore) |> ignore
    Assert.Throws<OverflowException>(fun () -> fixGenerator.GetConverter<struct (Fix * Fix * Fix * Fix)>() |> ignore) |> ignore
    Assert.Equal(0x4000_0000, alpha.Length)
    Assert.Equal(0x6000_0000, bravo.Length)
    ()
