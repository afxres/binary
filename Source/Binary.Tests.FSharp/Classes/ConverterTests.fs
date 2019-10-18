module Classes.ConverterTests

open Mikodev.Binary
open Mikodev.Binary.Abstractions
open System
open Xunit

let generator = new Generator()

[<Fact>]
let ``Object Converter`` () =
    let converter = generator.GetConverter<obj>()
    let source : obj = box (struct (3, 2.1))
    let mutable allocator = new Allocator()
    converter.ToBytes(&allocator, source)
    let buffer = allocator.ToArray()
    let result = generator.ToValue<struct (int * double)> buffer
    Assert.Equal(source, box result)
    ()

[<Fact>]
let ``Object Converter (to value)`` () =
    let converter = generator.GetConverter<obj>()
    let error = Assert.Throws<ArgumentException>(fun () -> converter.ToValue Array.empty<byte> |> ignore)
    Assert.Contains("Invalid type", error.Message)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let converter = generator.GetConverter<string> ()
    Assert.Equal("Converter(Length: 0, ItemType: System.String)", converter.ToString())
    ()

let test<'T> (item : 'T) =
    let mutable aa = new Allocator()
    let mutable ab = new Allocator()
    let ca = generator.GetConverter<'T>()
    let cb = ca :> IConverter
    ca.ToBytes(&aa, item)
    cb.ToBytes(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.ToArray(), ab.ToArray())

    let ba = aa.AsSpan()
    let bb = ab.AsSpan()
    let ra = ca.ToValue &ba
    let rb = cb.ToValue &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    ()

let testWithMark<'T> (item : 'T) =
    let mutable aa = new Allocator()
    let mutable ab = new Allocator()
    let ca = generator.GetConverter<'T>()
    let cb = ca :> IConverter
    ca.ToBytesWithMark(&aa, item)
    cb.ToBytesWithMark(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.ToArray(), ab.ToArray())

    let mutable ba = aa.AsSpan()
    let mutable bb = ab.AsSpan()
    let ra = ca.ToValueWithMark &ba
    let rb = cb.ToValueWithMark &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    Assert.Equal(0, ba.Length)
    Assert.Equal(0, bb.Length)
    ()

let testWithLengthPrefix<'T> (item : 'T) =
    let mutable aa = new Allocator()
    let mutable ab = new Allocator()
    let ca = generator.GetConverter<'T>()
    let cb = ca :> IConverter
    ca.ToBytesWithLengthPrefix(&aa, item)
    cb.ToBytesWithLengthPrefix(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.ToArray(), ab.ToArray())

    let mutable ba = aa.AsSpan()
    let mutable bb = ab.AsSpan()
    let ra = ca.ToValueWithLengthPrefix &ba
    let rb = cb.ToValueWithLengthPrefix &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    Assert.Equal(0, ba.Length)
    Assert.Equal(0, bb.Length)
    ()

[<Fact>]
let ``Interface Method 'ToBytes' And 'ToValue'`` () =
    test<int> 2048
    test<string> null
    test<string> "1024"
    test<Uri> null
    ()

[<Fact>]
let ``Interface Method 'ToBytesWithMark' And 'ToValueWithMark'`` () =
    testWithMark<int> 4096
    testWithMark<string> null
    testWithMark<string> "8192"
    testWithMark<Uri> null
    ()

[<Fact>]
let ``Interface Method 'ToBytesWithLengthPrefix' And 'ToValueWithLengthPrefix'`` () =
    testWithLengthPrefix<int> 16
    testWithLengthPrefix<string> null
    testWithLengthPrefix<string> "32"
    testWithLengthPrefix<Uri> null
    ()

type CustomConverter<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.ToBytes(_, _) = raise (NotSupportedException())

    override __.ToValue (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

    override __.ToBytesWithMark(_, _) = raise (NotSupportedException())

    override __.ToValueWithMark _ = raise (NotSupportedException())

type CustomConstantConverter<'T>(length : int) =
    inherit ConstantConverter<'T>(length)

    override __.ToBytes(_, _) = raise (NotSupportedException())

    override __.ToValue (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

    override __.ToBytesWithMark(_, _) = raise (NotSupportedException())

    override __.ToValueWithMark _ = raise (NotSupportedException())

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(127)>]
let ``Valid Converter Length`` (length : int) =
    let converter = new CustomConverter<obj>(length)
    Assert.Equal(length, converter.Length)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(33)>]
let ``Valid Constant Converter Length`` (length : int) =
    let converter = new CustomConstantConverter<obj>(length)
    Assert.Equal(length, converter.Length)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-65)>]
let ``Invalid Converter Length`` (length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> new CustomConverter<obj>(length) |> ignore)
    Assert.Equal("length", error.ParamName)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(-1)>]
[<InlineData(-255)>]
let ``Invalid Constant Converter Length`` (length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> new CustomConstantConverter<obj>(length) |> ignore)
    Assert.Equal("length", error.ParamName)
    ()

type CustomConstantConverterWithInvalidAllocation<'T>(length : int) =
    inherit ConstantConverter<'T>(length)

    override __.ToBytes(allocator, _) = let _ = AllocatorHelper.Allocate(&allocator, length + 1) in ()

    override __.ToValue (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

[<Theory>]
[<InlineData(1)>]
[<InlineData(127)>]
let ``Invalid Constant Converter Allocation`` (length : int) =
    let converter = new CustomConstantConverterWithInvalidAllocation<int>(length)
    let error = Assert.Throws<ArgumentException>(fun () -> converter.ToBytes(Unchecked.defaultof<int>) |> ignore)
    Assert.Equal("Maximum allocator capacity has been reached.", error.Message)
    ()

[<Fact>]
let ``To Value With Length Prefix (length prefix bytes invalid)`` () =
    let converter = generator.GetConverter<byte[]>()
    let message = "Length prefix bytes invalid."
    let bytes = Array.zeroCreate<byte> 0
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable span = ReadOnlySpan bytes in converter.ToValueWithLengthPrefix(&span) |> ignore)
    Assert.Equal(message, error.Message)
    ()
