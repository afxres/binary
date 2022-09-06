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

    static let MakeConverter (creatorName : string) (t : Type) (native : bool) =
        let types = typeof<IConverter>.Assembly.GetTypes()
        let value = types |> Array.filter (fun x -> x.Name = creatorName) |> Array.exactlyOne
        let method = value.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic) |> Array.filter (fun x -> x.ReturnType = typeof<IConverter> && x.Name.Contains("Invoke")) |> Array.exactlyOne
        let converter = method.Invoke(null, [| box t; box native |])
        converter :?> IConverter

    static let MakeConverters (t : Type) =
        EnsureEnumName t
        let on = MakeConverter "OldConverterCreator" t true
        let ol = MakeConverter "OldConverterCreator" t false
        let rn = MakeConverter "RawConverterCreator" t true
        let rl = MakeConverter "RawConverterCreator" t false
        Assert.Equal("NativeEndianConverter`1", on.GetType().Name)
        Assert.Equal("LittleEndianConverter`1", ol.GetType().Name)
        Assert.Matches("RawConverter.*NativeEndianRawConverter", rn.GetType().FullName)
        Assert.Matches("RawConverter.*LittleEndianRawConverter", rl.GetType().FullName)
        [ on; ol; rn; rl; ]

    static member ``Data Alpha`` : (obj array) seq = seq {
        [| box true |]
        [| box false |]
        [| box (byte 0xEF) |]
        [| box (sbyte 0x4F) |]
        [| box (char 'Z') |]
        [| box (int16 0x2333) |]
        [| box (uint16 0xFEFE) |]
        [| box (int32 0x13131313) |]
        [| box (uint32 0xEEFFAABB) |]
        [| box (int64 0x1122334455667788L) |]
        [| box (uint64 0xFFEEDDCCBBAA9988UL) |]
        [| box (single Math.E) |]
        [| box (double Math.PI) |]
        [| box (BitVector32(0x11223344)) |]
        [| box (BitVector32(0xAABBCCDD)) |]
    }

    static member ``Data Enum`` : (obj array) seq = seq {
        [| box EnumByte.Data |]
        [| box EnumSByte.Data |]
        [| box EnumInt16.Data |]
        [| box EnumUInt16.Data |]
        [| box EnumInt32.Data |]
        [| box EnumUInt32.Data |]
        [| box EnumInt64.Data |]
        [| box EnumUInt64.Data |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    [<MemberData("Data Enum")>]
    member __.``Encode Then Decode`` (item : 'T) =
        let converters = MakeConverters typeof<'T> |> Seq.cast<Converter<'T>>
        for converter in converters do
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
    member __.``Decode (not enough bytes)`` (item : 'T) =
        let converters = MakeConverters typeof<'T> |> Seq.cast<Converter<'T>>
        for converter in converters do
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
    member __.``Decode Auto (not enough bytes)`` (item : 'T) =
        let converters = MakeConverters typeof<'T> |> Seq.cast<Converter<'T>>
        for converter in converters do
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
    member __.``Decode (enough bytes)`` (item : 'T) =
        let converters = MakeConverters typeof<'T> |> Seq.cast<Converter<'T>>
        for converter in converters do
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
    member __.``Decode Auto (enough bytes)`` (item : 'T) =
        let converters = MakeConverters typeof<'T> |> Seq.cast<Converter<'T>>
        for converter in converters do
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
        [| box struct (1.0, 2.0, 3.0, 4.0) |]
        [| box struct (1, 2, 3, 4, 5, 6, 7, 8) |]
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
