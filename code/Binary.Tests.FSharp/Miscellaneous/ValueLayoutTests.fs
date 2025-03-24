module Miscellaneous.ValueLayoutTests

open Mikodev.Binary
open System
open System.Buffers.Binary
open Xunit

let generator = Generator.CreateDefault()

let Encode (item: 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Matches(".*\.LittleEndianConverter`1.*", converter.GetType().FullName)
    converter.Encode item

[<Theory>]
[<InlineData(0x1122s)>]
[<InlineData(0x7766s)>]
let ``Int16 Layout`` (item: Int16) =
    let origin = Array.zeroCreate<byte> sizeof<Int16>
    BinaryPrimitives.WriteInt16LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData(0xABCDus)>]
[<InlineData(0xDBCAus)>]
let ``UInt16 Layout`` (item: UInt16) =
    let origin = Array.zeroCreate<byte> sizeof<UInt16>
    BinaryPrimitives.WriteUInt16LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData(0x11223344)>]
[<InlineData(0x77665544)>]
let ``Int32 Layout`` (item: Int32) =
    let origin = Array.zeroCreate<byte> sizeof<Int32>
    BinaryPrimitives.WriteInt32LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData(0x89ABCDEFu)>]
[<InlineData(0xFEDCBA98u)>]
let ``UInt32 Layout`` (item: UInt32) =
    let origin = Array.zeroCreate<byte> sizeof<UInt32>
    BinaryPrimitives.WriteUInt32LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData(0x1122334455667788L)>]
[<InlineData(0x7766554433221100L)>]
let ``Int64 Layout`` (item: Int64) =
    let origin = Array.zeroCreate<byte> sizeof<Int64>
    BinaryPrimitives.WriteInt64LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData(0x0123456789ABCDEFUL)>]
[<InlineData(0xFEDCBA9876543210UL)>]
let ``UInt64 Layout`` (item: UInt64) =
    let origin = Array.zeroCreate<byte> sizeof<UInt64>
    BinaryPrimitives.WriteUInt64LittleEndian(origin.AsSpan(), item)
    let buffer = Encode item
    Assert.Equal<byte>(origin, buffer)
    ()

[<Theory>]
[<InlineData("048FED45-695E-48C5-B01F-1A3B9DCF3AF3")>]
[<InlineData("A042D19C-DB34-420D-861C-A783FCFFE938")>]
let ``Guid Layout`` (text: string) =
    let item = Guid.Parse text
    let origin = item.ToByteArray()
    let converter = generator.GetConverter<Guid>()
    Assert.Matches(".*\.GuidConverter$", converter.GetType().FullName)
    let buffer = converter.Encode item
    Assert.Equal<byte>(origin, buffer)
    ()
