﻿namespace Classes

open Mikodev.Binary
open System
open System.Reflection
open Xunit

type CustomConverter<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

    override __.EncodeAuto(_, _) = raise (NotSupportedException())

    override __.DecodeAuto _ = raise (NotSupportedException())

type CustomConverterWithInvalidAllocation<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(allocator, _) = let _ = AllocatorHelper.Append(&allocator, length + 1, null :> obj, fun a b -> ()) in ()

    override __.Decode (_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type CustomConverterWithLength<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(_, _) = raise (NotSupportedException("Text a"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("Text b"))

    override __.EncodeWithLengthPrefix(_, _) = raise (NotSupportedException("Text c"))

    override __.DecodeWithLengthPrefix _ = raise (NotSupportedException("Text d"))

type CustomConverterAllOverride<'T>(length : int) =
    inherit Converter<'T>(length)

    override __.Encode(_, _) = raise (NotSupportedException("01"))

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException("02"))

    override __.EncodeAuto(_, _) = raise (NotSupportedException("03"))

    override __.DecodeAuto _ = raise (NotSupportedException("04"))

    override __.EncodeWithLengthPrefix(_, _) = raise (NotSupportedException("05"))

    override __.DecodeWithLengthPrefix _ = raise (NotSupportedException("06"))

    override __.Encode(_) = raise (NotSupportedException("07"))

    override __.Decode(_ : byte array) : 'T = raise (NotSupportedException("08"))

type ConverterTests () =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Object Converter`` () =
        let converter = generator.GetConverter<obj>()
        let source : obj = box (struct (3, 2.1))
        let mutable allocator = new Allocator()
        converter.Encode(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()
        let result = generator.Decode<struct (int * double)> buffer
        Assert.Equal(source, box result)
        ()

    [<Fact>]
    member __.``Object Converter (encode object instance)`` () =
        let converter = generator.GetConverter<obj>()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Encode (new obj()) |> ignore)
        Assert.Equal("Invalid system type: System.Object", error.Message)
        ()

    [<Fact>]
    member __.``Object Converter (encode null)`` () =
        let converter = generator.GetConverter<obj>()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Encode null |> ignore)
        Assert.Equal("Can not get type of null object.", error.Message)
        ()

    [<Fact>]
    member __.``Object Converter (decode)`` () =
        let converter = generator.GetConverter<obj>()
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Decode Array.empty<byte> |> ignore)
        Assert.Equal("Invalid system type: System.Object", error.Message)
        ()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| typeof<int>; 4 |]
            yield [| typeof<double>; 8 |]
            yield [| typeof<string>; 0 |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``To String (debug)`` (t : Type, length : int) =
        let converter = generator.GetConverter t
        let message = sprintf "Converter<%s>(Length: %d)" t.Name length
        Assert.Equal(message, converter.ToString())
        ()

    member __.Test<'T> (item : 'T) =
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

    member __.TestAuto<'T> (item : 'T) =
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

    member __.TestWithLengthPrefix<'T> (item : 'T) =
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
    member me.``Interface Method 'Encode' And 'Decode'`` () =
        me.Test<int> 2048
        me.Test<string> null
        me.Test<string> "1024"
        me.Test<Uri> null
        ()

    [<Fact>]
    member me.``Interface Method 'EncodeAuto' And 'DecodeAuto'`` () =
        me.TestAuto<int> 4096
        me.TestAuto<string> null
        me.TestAuto<string> "8192"
        me.TestAuto<Uri> null
        ()

    [<Fact>]
    member me.``Interface Method 'EncodeWithLengthPrefix' And 'DecodeWithLengthPrefix'`` () =
        me.TestWithLengthPrefix<int> 16
        me.TestWithLengthPrefix<string> null
        me.TestWithLengthPrefix<string> "32"
        me.TestWithLengthPrefix<Uri> null
        ()

    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(1)>]
    [<InlineData(127)>]
    [<InlineData(1024)>]
    member __.``Valid Converter Length`` (length : int) =
        let converter = new CustomConverter<obj>(length)
        Assert.Equal(length, converter.Length)
        ()

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(-65)>]
    member __.``Invalid Converter Length`` (length : int) =
        let constructorInfo = typeof<Converter<int>>.GetConstructor(BindingFlags.Instance ||| BindingFlags.NonPublic, null, [| typeof<int> |], null)
        let parameter = constructorInfo.GetParameters() |> Array.exactlyOne
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> new CustomConverter<obj>(length) |> ignore)
        Assert.Equal("length", parameter.Name)
        Assert.Equal("length", error.ParamName)
        Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
        ()

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(127)>]
    member __.``Invalid Converter Allocation`` (length : int) =
        let converter = new CustomConverterWithInvalidAllocation<int>(length)
        let error = Assert.Throws<ArgumentException>(fun () -> converter.Encode(Unchecked.defaultof<int>) |> ignore)
        Assert.Equal("Maximum capacity has been reached.", error.Message)
        ()

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    member __.``Auto Method For Constant Length Converter`` (length : int) =
        let converter = CustomConverterWithLength<int> length
        let alpha = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, 0))
        let bravo = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan (Array.empty<byte>) in converter.DecodeAuto &span |> ignore)
        Assert.Equal("Text a", alpha.Message)
        Assert.Equal("Text b", bravo.Message)
        ()

    [<Fact>]
    member __.``Auto Method For Variable Length Converter`` () =
        let converter = CustomConverterWithLength<int> 0
        let alpha = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, 0))
        let bravo = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan (Array.empty<byte>) in converter.DecodeAuto &span |> ignore)
        Assert.Equal("Text c", alpha.Message)
        Assert.Equal("Text d", bravo.Message)
        ()

    [<Fact>]
    member __.``Interface Method All Forwarded`` () =
        let converter = new CustomConverterAllOverride<obj>(0) :> IConverter
        let e1 = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = new Allocator() in converter.Encode(&allocator, null) |> ignore)
        let ea = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = new Allocator() in converter.EncodeAuto(&allocator, null) |> ignore)
        let ew = Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = new Allocator() in converter.EncodeWithLengthPrefix(&allocator, null) |> ignore)
        let e2 = Assert.Throws<NotSupportedException>(fun () -> converter.Encode(null) |> ignore)
        let d1 = Assert.Throws<NotSupportedException>(fun () -> let span = ReadOnlySpan<byte>() in converter.Decode &span |> ignore)
        let da = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan<byte>() in converter.DecodeAuto &span |> ignore)
        let dw = Assert.Throws<NotSupportedException>(fun () -> let mutable span = ReadOnlySpan<byte>() in converter.DecodeWithLengthPrefix &span |> ignore)
        let d2 = Assert.Throws<NotSupportedException>(fun () -> converter.Decode Array.empty<byte> |> ignore)
        Assert.Equal("01", e1.Message)
        Assert.Equal("03", ea.Message)
        Assert.Equal("05", ew.Message)
        Assert.Equal("07", e2.Message)
        Assert.Equal("02", d1.Message)
        Assert.Equal("04", da.Message)
        Assert.Equal("06", dw.Message)
        Assert.Equal("08", d2.Message)
        let errors = [| e1; ea; ew; e2; d1; da; dw; d2 |]
        Assert.All(errors, fun x -> Assert.Contains("Mikodev.Binary.IConverter.", x.StackTrace))
        ()
