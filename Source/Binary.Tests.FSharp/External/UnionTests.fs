﻿module External.UnionTests

open Mikodev.Binary
open System
open System.Reflection
open Xunit

let generator = Generator.CreateDefaultBuilder()
                    .AddFSharpConverterCreators()
                    .Build();

let converter<'a> =
    let value = generator.GetConverter(typeof<'a>)
    Assert.Contains("UnionConverter", value.GetType().Name)
    value :?> Converter<'a>

let encode<'a> (c : Converter<'a>) v =
    let mutable allocator = new Allocator()
    c.Encode(&allocator, v)
    allocator.AsSpan().ToArray()

let encodeAuto<'a> (c : Converter<'a>) v =
    let mutable allocator = new Allocator()
    c.EncodeAuto(&allocator, v)
    allocator.AsSpan().ToArray()

let decode<'a> (c : Converter<'a>) buffer =
    let span = new ReadOnlySpan<byte>(buffer)
    c.Decode &span

let decodeAuto<'a> (c : Converter<'a>) buffer =
    let mutable span = new ReadOnlySpan<byte>(buffer)
    c.DecodeAuto &span

let test<'a> ls ll (v : 'a) =
    // test encode, decode
    let c = converter<'a>
    let ba = encode c v
    Assert.Equal(ls, Array.length ba)
    let ra = decode c ba
    Assert.Equal(v, ra)

    // test encode, decode (bytes)
    let bc = c.Encode v
    Assert.Equal(ls, Array.length bc)
    Assert.Equal<byte>(ba, bc)
    let rc = c.Decode bc
    Assert.Equal(v, rc)

    // test encode, decode (auto)
    let bb = encodeAuto c v
    Assert.Equal(ll, Array.length bb)
    let rb = decodeAuto c bb
    Assert.Equal(v, rb)
    ()

[<Fact>]
let ``Option`` () =
    Some 1 |> test 5 5
    Some "empty" |> test 6 7
    None |> test<int option> 1 1
    None |> test<string option> 1 1
    ()

[<Fact>]
let ``ValueOption`` () =
    ValueSome 8uy |> test 2 2
    ValueSome "nullptr" |> test 8 9
    ValueNone |> test<int16 voption> 1 1
    ValueNone |> test<string voption> 1 1
    ()

[<Fact>]
let ``Result`` () =
    Ok 1024 |> test 5 5
    Ok "32" |> test 3 4
    Error 6 |> test 5 5
    Error "error" |> test 6 7
    ()

[<Fact>]
let ``Choice 2`` () =
    Choice1Of2 2L |> test 9 9
    Choice2Of2 "test" |> test 5 6
    ()

[<Theory>]
[<InlineData(2)>]
[<InlineData(128)>]
[<InlineData(4096)>]
let ``Invalid Tag (decode & decode auto)`` (tag : int) =
    let converter = generator.GetConverter<int option>()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)

    let mutable allocator = Allocator()
    PrimitiveHelper.EncodeNumber(&allocator, tag)
    let bytes = allocator.AsSpan().ToArray()

    let message = sprintf "Invalid union tag '%d', type: %O" (int tag) typeof<int option>
    let alpha = Assert.Throws<ArgumentException>(fun () -> decode<int option> converter bytes |> ignore)
    let bravo = Assert.Throws<ArgumentException>(fun () -> decodeAuto<int option> converter bytes |> ignore)
    Assert.Null(alpha.ParamName)
    Assert.Null(bravo.ParamName)
    Assert.StartsWith(message, alpha.Message)
    Assert.StartsWith(message, bravo.Message)
    ()

[<Struct>]
type Box =
    | One of one : int
    | Two of two : string

[<Theory>]
[<InlineData(2)>]
[<InlineData(255)>]
[<InlineData(65537)>]
let ``Invalid Tag With Fake Union Type (hack)`` (tag : int) =
    let converter = generator.GetConverter<Box>()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)

    let boxed = box (One 10)
    let field = boxed.GetType().GetField("_tag", BindingFlags.Instance ||| BindingFlags.NonPublic)
    Assert.NotNull(field)
    // hack tag member!
    field.SetValue(boxed, tag)

    let value = unbox<Box> boxed
    let message = sprintf "Invalid union tag '%d', type: %O" (int tag) typeof<Box>
    let alpha = Assert.Throws<ArgumentException>(fun () -> encode<Box> converter value |> ignore)
    let bravo = Assert.Throws<ArgumentException>(fun () -> encodeAuto<Box> converter value |> ignore)
    Assert.Null(alpha.ParamName)
    Assert.Null(bravo.ParamName)
    Assert.StartsWith(message, alpha.Message)
    Assert.StartsWith(message, bravo.Message)
    ()

type ABC =
    | A of int * string
    | B of int * int * string
    | C of string

[<Fact>]
let ``Invalid Null Value (encode & encode auto)`` () =
    let converter = generator.GetConverter<ABC>()
    Assert.StartsWith("UnionConverter`1", converter.GetType().Name)

    let message = sprintf "Union can not be null, type: %O" typeof<ABC>
    let alpha = Assert.Throws<ArgumentNullException>(fun () -> encode converter Unchecked.defaultof<ABC> |> ignore)
    let bravo = Assert.Throws<ArgumentNullException>(fun () -> encodeAuto converter Unchecked.defaultof<ABC> |> ignore)
    Assert.Equal("item", alpha.ParamName)
    Assert.Equal("item", bravo.ParamName)
    Assert.StartsWith(message, alpha.Message)
    Assert.StartsWith(message, bravo.Message)
    ()

type X256 =
    | X00 | X01 | X02 | X03 | X04 | X05 | X06 | X07 | X08 | X09 | X0A | X0B | X0C | X0D | X0E | X0F
    | X10 | X11 | X12 | X13 | X14 | X15 | X16 | X17 | X18 | X19 | X1A | X1B | X1C | X1D | X1E | X1F
    | X20 | X21 | X22 | X23 | X24 | X25 | X26 | X27 | X28 | X29 | X2A | X2B | X2C | X2D | X2E | X2F
    | X30 | X31 | X32 | X33 | X34 | X35 | X36 | X37 | X38 | X39 | X3A | X3B | X3C | X3D | X3E | X3F
    | X40 | X41 | X42 | X43 | X44 | X45 | X46 | X47 | X48 | X49 | X4A | X4B | X4C | X4D | X4E | X4F
    | X50 | X51 | X52 | X53 | X54 | X55 | X56 | X57 | X58 | X59 | X5A | X5B | X5C | X5D | X5E | X5F
    | X60 | X61 | X62 | X63 | X64 | X65 | X66 | X67 | X68 | X69 | X6A | X6B | X6C | X6D | X6E | X6F
    | X70 | X71 | X72 | X73 | X74 | X75 | X76 | X77 | X78 | X79 | X7A | X7B | X7C | X7D | X7E | X7F
    | X80 | X81 | X82 | X83 | X84 | X85 | X86 | X87 | X88 | X89 | X8A | X8B | X8C | X8D | X8E | X8F
    | X90 | X91 | X92 | X93 | X94 | X95 | X96 | X97 | X98 | X99 | X9A | X9B | X9C | X9D | X9E | X9F
    | XA0 | XA1 | XA2 | XA3 | XA4 | XA5 | XA6 | XA7 | XA8 | XA9 | XAA | XAB | XAC | XAD | XAE | XAF
    | XB0 | XB1 | XB2 | XB3 | XB4 | XB5 | XB6 | XB7 | XB8 | XB9 | XBA | XBB | XBC | XBD | XBE | XBF
    | XC0 | XC1 | XC2 | XC3 | XC4 | XC5 | XC6 | XC7 | XC8 | XC9 | XCA | XCB | XCC | XCD | XCE | XCF
    | XD0 | XD1 | XD2 | XD3 | XD4 | XD5 | XD6 | XD7 | XD8 | XD9 | XDA | XDB | XDC | XDD | XDE | XDF
    | XE0 | XE1 | XE2 | XE3 | XE4 | XE5 | XE6 | XE7 | XE8 | XE9 | XEA | XEB | XEC | XED | XEE | XEF
    | XF0 | XF1 | XF2 | XF3 | XF4 | XF5 | XF6 | XF7 | XF8 | XF9 | XFA | XFB | XFC | XFD | XFE | XFF

[<Fact>]
let ``Union With 256 Cases`` () =
    test 1 1 X256.X00
    test 2 2 X256.XFF
    test 2 2 X256.X80
    test 1 1 X256.X3F
    ()

type X257 =
    | X00 | X01 | X02 | X03 | X04 | X05 | X06 | X07 | X08 | X09 | X0A | X0B | X0C | X0D | X0E | X0F
    | X10 | X11 | X12 | X13 | X14 | X15 | X16 | X17 | X18 | X19 | X1A | X1B | X1C | X1D | X1E | X1F
    | X20 | X21 | X22 | X23 | X24 | X25 | X26 | X27 | X28 | X29 | X2A | X2B | X2C | X2D | X2E | X2F
    | X30 | X31 | X32 | X33 | X34 | X35 | X36 | X37 | X38 | X39 | X3A | X3B | X3C | X3D | X3E | X3F
    | X40 | X41 | X42 | X43 | X44 | X45 | X46 | X47 | X48 | X49 | X4A | X4B | X4C | X4D | X4E | X4F
    | X50 | X51 | X52 | X53 | X54 | X55 | X56 | X57 | X58 | X59 | X5A | X5B | X5C | X5D | X5E | X5F
    | X60 | X61 | X62 | X63 | X64 | X65 | X66 | X67 | X68 | X69 | X6A | X6B | X6C | X6D | X6E | X6F
    | X70 | X71 | X72 | X73 | X74 | X75 | X76 | X77 | X78 | X79 | X7A | X7B | X7C | X7D | X7E | X7F
    | X80 | X81 | X82 | X83 | X84 | X85 | X86 | X87 | X88 | X89 | X8A | X8B | X8C | X8D | X8E | X8F
    | X90 | X91 | X92 | X93 | X94 | X95 | X96 | X97 | X98 | X99 | X9A | X9B | X9C | X9D | X9E | X9F
    | XA0 | XA1 | XA2 | XA3 | XA4 | XA5 | XA6 | XA7 | XA8 | XA9 | XAA | XAB | XAC | XAD | XAE | XAF
    | XB0 | XB1 | XB2 | XB3 | XB4 | XB5 | XB6 | XB7 | XB8 | XB9 | XBA | XBB | XBC | XBD | XBE | XBF
    | XC0 | XC1 | XC2 | XC3 | XC4 | XC5 | XC6 | XC7 | XC8 | XC9 | XCA | XCB | XCC | XCD | XCE | XCF
    | XD0 | XD1 | XD2 | XD3 | XD4 | XD5 | XD6 | XD7 | XD8 | XD9 | XDA | XDB | XDC | XDD | XDE | XDF
    | XE0 | XE1 | XE2 | XE3 | XE4 | XE5 | XE6 | XE7 | XE8 | XE9 | XEA | XEB | XEC | XED | XEE | XEF
    | XF0 | XF1 | XF2 | XF3 | XF4 | XF5 | XF6 | XF7 | XF8 | XF9 | XFA | XFB | XFC | XFD | XFE | XFF
    | XWhat

[<Fact>]
let ``Union With 257 Cases`` () =
    test 1 1 X257.X00
    test 2 2 X257.XFF
    test 2 2 X257.XWhat
    test 2 2 X257.X80
    test 1 1 X257.X3F
    ()
