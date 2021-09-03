namespace Endianness

open Mikodev.Binary
open System
open System.Collections.Specialized
open System.Drawing
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.Serialization
open Xunit

type EnumByte =
    | Data = 0xABuy

type EnumSByte =
    | Data = 0x4Dy

type EnumInt16 =
    | Data = 0x2020s

type EnumUInt16 =
    | Data = 0xEFFEus

type EnumInt32 =
    | Data = 0x22446688l

type EnumUInt32 =
    | Data = 0xBBAA8877ul

type EnumInt64 =
    | Data = 0x778899AABBCCDDEEL

type EnumUInt64 =
    | Data = 0xEEDDCCBBAA998877UL

type ConverterTests() =
    static let EnsureEnumName (t : Type) =
        if t.IsEnum && t.Assembly = typeof<ConverterTests>.Assembly then
            let underlying = Enum.GetUnderlyingType t
            let expectedName = sprintf "Enum%s" underlying.Name
            Assert.Equal(expectedName, t.Name)
        ()

    static let MakeNativeEndianConverter (t : Type) =
        EnsureEnumName t
        let generator = Generator.CreateDefault()
        let converter = generator.GetConverter t
        Assert.Equal("NativeEndianConverter`1", converter.GetType().Name)
        converter

    static let MakeLittleEndianConverter (t : Type) =
        EnsureEnumName t
        let types = typeof<IConverter>.Assembly.GetTypes()
        let value = types |> Array.filter (fun x -> x.Name = "FallbackEndiannessMethods") |> Array.exactlyOne
        let method = value.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic) |> Array.filter (fun x -> x.ReturnType = typeof<IConverter> && x.Name.Contains("Invoke")) |> Array.exactlyOne
        let converter = method.Invoke(null, [| box t; box false |])
        Assert.Equal("LittleEndianConverter`1", converter.GetType().Name)
        converter :?> IConverter

    static let NativeEndianConverter = Func<Type, IConverter>(MakeNativeEndianConverter)

    static let LittleEndianConverter = Func<Type, IConverter>(MakeLittleEndianConverter)

    static member ``Data Alpha`` : (obj array) seq = seq {
        [| box NativeEndianConverter; box true |]
        [| box NativeEndianConverter; box false |]
        [| box NativeEndianConverter; box (byte 0xEF) |]
        [| box NativeEndianConverter; box (sbyte 0x4F) |]
        [| box NativeEndianConverter; box (char 'Z') |]
        [| box NativeEndianConverter; box (int16 0x2333) |]
        [| box NativeEndianConverter; box (uint16 0xFEFE) |]
        [| box NativeEndianConverter; box (int32 0x13131313) |]
        [| box NativeEndianConverter; box (uint32 0xEEFFAABB) |]
        [| box NativeEndianConverter; box (int64 0x1122334455667788L) |]
        [| box NativeEndianConverter; box (uint64 0xFFEEDDCCBBAA9988UL) |]
        [| box NativeEndianConverter; box (single Math.E) |]
        [| box NativeEndianConverter; box (double Math.PI) |]
        [| box NativeEndianConverter; box (BitVector32(0x11223344)) |]
        [| box NativeEndianConverter; box (BitVector32(0xAABBCCDD)) |]

        [| box LittleEndianConverter; box true |]
        [| box LittleEndianConverter; box false |]
        [| box LittleEndianConverter; box (byte 0xEF) |]
        [| box LittleEndianConverter; box (sbyte 0x4F) |]
        [| box LittleEndianConverter; box (char 'Z') |]
        [| box LittleEndianConverter; box (int16 0x2333) |]
        [| box LittleEndianConverter; box (uint16 0xFEFE) |]
        [| box LittleEndianConverter; box (int32 0x13131313) |]
        [| box LittleEndianConverter; box (uint32 0xEEFFAABB) |]
        [| box LittleEndianConverter; box (int64 0x1122334455667788L) |]
        [| box LittleEndianConverter; box (uint64 0xFFEEDDCCBBAA9988UL) |]
        [| box LittleEndianConverter; box (single Math.E) |]
        [| box LittleEndianConverter; box (double Math.PI) |]
        [| box LittleEndianConverter; box (BitVector32(0x11223344)) |]
        [| box LittleEndianConverter; box (BitVector32(0xAABBCCDD)) |]
    }

    static member ``Data Enum`` : (obj array) seq = seq {
        [| box NativeEndianConverter; box EnumByte.Data |]
        [| box NativeEndianConverter; box EnumSByte.Data |]
        [| box NativeEndianConverter; box EnumInt16.Data |]
        [| box NativeEndianConverter; box EnumUInt16.Data |]
        [| box NativeEndianConverter; box EnumInt32.Data |]
        [| box NativeEndianConverter; box EnumUInt32.Data |]
        [| box NativeEndianConverter; box EnumInt64.Data |]
        [| box NativeEndianConverter; box EnumUInt64.Data |]

        [| box LittleEndianConverter; box EnumByte.Data |]
        [| box LittleEndianConverter; box EnumSByte.Data |]
        [| box LittleEndianConverter; box EnumInt16.Data |]
        [| box LittleEndianConverter; box EnumUInt16.Data |]
        [| box LittleEndianConverter; box EnumInt32.Data |]
        [| box LittleEndianConverter; box EnumUInt32.Data |]
        [| box LittleEndianConverter; box EnumInt64.Data |]
        [| box LittleEndianConverter; box EnumUInt64.Data |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Encode Then Decode`` (f : Func<Type, IConverter>, item : 'T) =
        let converter = f.Invoke(typeof<'T>) :?> Converter<'T>
        let mutable allocator = Allocator()
        converter.Encode(&allocator, item)
        let buffer = allocator.ToArray()
        Assert.Equal(Unsafe.SizeOf<'T>(), buffer.Length)

        let span = ReadOnlySpan buffer
        let result = converter.Decode &span
        Assert.Equal<'T>(item, result)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Decode (not enough bytes)`` (f : Func<Type, IConverter>, item : 'T) =
        let converter = f.Invoke(typeof<'T>) :?> Converter<'T>
        let mutable allocator = Allocator()
        converter.Encode(&allocator, item)
        let buffer = allocator.ToArray()
        Assert.Equal(Unsafe.SizeOf<'T>(), buffer.Length)

        let mutable list = []
        for i = 0 to buffer.Length - 1 do
            let slice = Array.sub buffer 0 i
            let error = Assert.Throws<ArgumentException>(fun () -> let span = ReadOnlySpan slice in converter.Decode &span |> ignore)
            list <- error :: list

        Assert.Equal(Unsafe.SizeOf<'T>(), List.length list)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.All(list, fun x -> Assert.Equal(message, x.Message))
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Decode Auto (not enough bytes)`` (f : Func<Type, IConverter>, item : 'T) =
        let converter = f.Invoke(typeof<'T>) :?> Converter<'T>
        let mutable allocator = Allocator()
        converter.Encode(&allocator, item)
        let buffer = allocator.ToArray()
        Assert.Equal(Unsafe.SizeOf<'T>(), buffer.Length)

        let mutable list = []
        for i = 0 to buffer.Length - 1 do
            let slice = Array.sub buffer 0 i
            let error = Assert.Throws<ArgumentException>(fun () -> let mutable span = ReadOnlySpan slice in converter.DecodeAuto &span |> ignore)
            list <- error :: list

        Assert.Equal(Unsafe.SizeOf<'T>(), List.length list)
        let message = "Not enough bytes or byte sequence invalid."
        Assert.All(list, fun x -> Assert.Equal(message, x.Message))
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Decode (enough bytes)`` (f : Func<Type, IConverter>, item : 'T) =
        let converter = f.Invoke(typeof<'T>) :?> Converter<'T>
        let mutable allocator = Allocator()
        converter.Encode(&allocator, item)
        let buffer = allocator.ToArray()
        Assert.Equal(Unsafe.SizeOf<'T>(), buffer.Length)

        for i = 0 to 8 do
            let array = Array.concat [| buffer; Array.zeroCreate i |]
            let span = ReadOnlySpan array
            let result = converter.Decode &span
            Assert.Equal<'T>(item, result)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Decode Auto (enough bytes)`` (f : Func<Type, IConverter>, item : 'T) =
        let converter = f.Invoke(typeof<'T>) :?> Converter<'T>
        let mutable allocator = Allocator()
        converter.Encode(&allocator, item)
        let buffer = allocator.ToArray()
        Assert.Equal(Unsafe.SizeOf<'T>(), buffer.Length)

        for i = 0 to 8 do
            let array = Array.concat [| buffer; Array.zeroCreate i |]
            let mutable span = ReadOnlySpan array
            let result = converter.DecodeAuto &span
            Assert.Equal<'T>(item, result)
            Assert.Equal(i, span.Length)
        ()

    static member ``Data Not Supported`` : (obj array) seq = seq {
        [| box (Rectangle(1, 1, 3, 4)) |]
        [| box DateTimeOffset.MinValue |]
    }

    [<Theory>]
    [<MemberData("Data Not Supported")>]
    member __.``Encode Not Supported (little endian converter)`` (item : 'T) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "LittleEndianConverter`1") |> Array.exactlyOne
        let c = FormatterServices.GetSafeUninitializedObject(t.MakeGenericType(typeof<'T>)) :?> Converter<'T>
        Assert.Throws<NotSupportedException>(fun () -> let mutable allocator = Allocator() in c.Encode(&allocator, item)) |> ignore
        ()

    [<Theory>]
    [<MemberData("Data Not Supported")>]
    member __.``Decode Not Supported (little endian converter)`` (_ : 'T) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "LittleEndianConverter`1") |> Array.exactlyOne
        let c = FormatterServices.GetSafeUninitializedObject(t.MakeGenericType(typeof<'T>)) :?> Converter<'T>
        Assert.Throws<NotSupportedException>(fun () -> let span = ReadOnlySpan<byte>(Array.zeroCreate 1024) in c.Decode(&span) |> ignore) |> ignore
        ()
