module Classes.ConverterTests

open Mikodev.Binary
open System
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

[<Fact>]
let ``Object Converter`` () =
    let converter = generator.GetConverter<obj>()
    let source : obj = box (struct (3, 2.1))
    let mutable allocator = new Allocator()
    converter.Encode(&allocator, source)
    let buffer = allocator.AsSpan().ToArray()
    let result = generator.Decode<struct (int * double)> buffer
    Assert.Equal(source, box result)
    ()

[<Fact>]
let ``Object Converter (decode)`` () =
    let converter = generator.GetConverter<obj>()
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode Array.empty<byte> |> ignore)
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
    ca.Encode(&aa, item)
    cb.Encode(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.AsSpan().ToArray(), ab.AsSpan().ToArray())

    let ba = aa.AsSpan()
    let bb = ab.AsSpan()
    let ra = ca.Decode &ba
    let rb = cb.Decode &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    ()

let testAuto<'T> (item : 'T) =
    let mutable aa = new Allocator()
    let mutable ab = new Allocator()
    let ca = generator.GetConverter<'T>()
    let cb = ca :> IConverter
    ca.EncodeAuto(&aa, item)
    cb.EncodeAuto(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.AsSpan().ToArray(), ab.AsSpan().ToArray())

    let mutable ba = aa.AsSpan()
    let mutable bb = ab.AsSpan()
    let ra = ca.DecodeAuto &ba
    let rb = cb.DecodeAuto &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    Assert.Equal(0, ba.Length)
    Assert.Equal(0, bb.Length)
    ()

let testWithLengthPrefix<'T> (item : 'T) =
    let mutable aa = new Allocator()
    let mutable ab = new Allocator()
    let ca = generator.GetConverter<'T>()
    let cb = ca :> IConverter
    ca.EncodeWithLengthPrefix(&aa, item)
    cb.EncodeWithLengthPrefix(&ab, box item)
    Assert.Equal(aa.Length, ab.Length)
    Assert.Equal<byte>(aa.AsSpan().ToArray(), ab.AsSpan().ToArray())

    let mutable ba = aa.AsSpan()
    let mutable bb = ab.AsSpan()
    let ra = ca.DecodeWithLengthPrefix &ba
    let rb = cb.DecodeWithLengthPrefix &bb |> Unchecked.unbox<'T>
    Assert.Equal(ra, rb)
    Assert.Equal(0, ba.Length)
    Assert.Equal(0, bb.Length)
    ()

[<Fact>]
let ``Interface Method 'Encode' And 'Decode'`` () =
    test<int> 2048
    test<string> null
    test<string> "1024"
    test<Uri> null
    ()

[<Fact>]
let ``Interface Method 'EncodeAuto' And 'DecodeAuto'`` () =
    testAuto<int> 4096
    testAuto<string> null
    testAuto<string> "8192"
    testAuto<Uri> null
    ()

[<Fact>]
let ``Interface Method 'EncodeWithLengthPrefix' And 'DecodeWithLengthPrefix'`` () =
    testWithLengthPrefix<int> 16
    testWithLengthPrefix<string> null
    testWithLengthPrefix<string> "32"
    testWithLengthPrefix<Uri> null
    ()

type CustomConverter<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

    override __.EncodeAuto(_, _) = raise (NotSupportedException())

    override __.DecodeAuto _ = raise (NotSupportedException())

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(127)>]
[<InlineData(1024)>]
let ``Valid Converter Length`` (length : int) =
    let converter = new CustomConverter<obj>(length)
    Assert.Equal(length, converter.Length)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-65)>]
let ``Invalid Converter Length`` (length : int) =
    let constructorInfo = typeof<Converter<int>>.GetConstructor(BindingFlags.Instance ||| BindingFlags.NonPublic, null, [| typeof<int> |], null)
    let parameter = constructorInfo.GetParameters() |> Array.exactlyOne
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> new CustomConverter<obj>(length) |> ignore)
    Assert.Equal("length", parameter.Name)
    Assert.Equal("length", error.ParamName)
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    ()

type CustomConverterWithInvalidAllocation<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(allocator, _) = let _ = AllocatorHelper.Append(&allocator, length + 1, null :> obj, fun a b -> ()) in ()

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

[<Theory>]
[<InlineData(1)>]
[<InlineData(127)>]
let ``Invalid Converter Allocation`` (length : int) =
    let converter = new CustomConverterWithInvalidAllocation<int>(length)
    let error = Assert.Throws<ArgumentException>(fun () -> converter.Encode(Unchecked.defaultof<int>) |> ignore)
    Assert.Equal("Maximum allocator capacity has been reached.", error.Message)
    ()

type CustomConverterWithLength<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(_, _) = raise (NotSupportedException("Text a"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("Text b"))

    override __.EncodeWithLengthPrefix(_, _) = raise (NotSupportedException("Text c"))

    override __.DecodeWithLengthPrefix _ = raise (NotSupportedException("Text d"))

[<Theory>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Auto Method For Constant Length Converter`` (length : int) =
    let converter = CustomConverterWithLength<int> length
    let alpha = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, 0))
    let bravo = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan (Array.empty<byte>) in converter.DecodeAuto &span |> ignore)
    Assert.Equal("Text a", alpha.Message)
    Assert.Equal("Text b", bravo.Message)
    ()

[<Fact>]
let ``Auto Method For Variable Length Converter`` () =
    let converter = CustomConverterWithLength<int> 0
    let alpha = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, 0))
    let bravo = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan (Array.empty<byte>) in converter.DecodeAuto &span |> ignore)
    Assert.Equal("Text c", alpha.Message)
    Assert.Equal("Text d", bravo.Message)
    ()
