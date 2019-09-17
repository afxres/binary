module Primitives.ValueTypeTests

open Mikodev.Binary
open Mikodev.Binary.Abstractions
open System
open Xunit

let random = new Random()

let generator = new Generator()

let randomCount = 64

let randomNumber () : uint64 =
    let buffer = Array.zeroCreate<byte>(sizeof<uint64>)
    random.NextBytes buffer
    let number = BitConverter.ToUInt64(buffer, 0)
    number

let testWithSpan (value : 'a) (size : int) =
    let bufferOrigin = generator.ToBytes value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = new Allocator()
    converter.ToBytes(&allocator, value)
    let buffer = allocator.ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)
    Assert.Equal(size, buffer.Length)

    let span = ReadOnlySpan buffer
    let result = converter.ToValue &span
    Assert.Equal<'a>(value, result)
    ()

let testWithBytes (value : 'a) (size : int) =
    let bufferOrigin = generator.ToBytes value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.ToBytes value
    Assert.Equal<byte>(bufferOrigin, buffer)
    Assert.Equal(size, buffer.Length)

    let result = converter.ToValue buffer
    Assert.Equal<'a>(value, result)
    ()

let testWithLengthPrefix (value : 'a) (size : int) =
    let bufferLength = generator.ToBytes size
    let bufferOrigin = generator.ToBytes value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = new Allocator()
    converter.ToBytesWithLengthPrefix(&allocator, value)
    let buffer = allocator.ToArray()
    Assert.Equal(size + sizeof<int>, buffer.Length)
    Assert.Equal<byte>(bufferLength, buffer |> Array.take sizeof<int>)
    Assert.Equal<byte>(bufferOrigin, buffer |> Array.skip sizeof<int>)

    let mutable span = ReadOnlySpan buffer
    let result = converter.ToValueWithLengthPrefix(&span)
    Assert.True(span.IsEmpty)
    Assert.Equal<'a>(value, result)
    ()

let testExplicit (value : 'a) (size : int) =
    // convert via Generator
    let buffer = generator.ToBytes value
    Assert.Equal(size, buffer.Length)

    let result : 'a = generator.ToValue buffer
    Assert.Equal<'a>(value, result)
    
    let converter = generator.GetConverter<'a>() :?> ConstantConverter<'a>
    Assert.Equal(size, converter.Length)

    // convert via Converter
    testWithSpan value size
    // convert via bytes methods
    testWithBytes value size
    // convert with length prefix
    testWithLengthPrefix value size
    ()
    
let test (value : 'a when 'a : unmanaged) = testExplicit value sizeof<'a>
    
[<Fact>]
let ``Int & UInit 16, 32, 64`` () =
    for i = 1 to randomCount do
        let i16 : int16 = int16(randomNumber())
        let i32 : int32 = int32(randomNumber())
        let i64 : int64 = int64(randomNumber())
        let u16 : uint16 = uint16(randomNumber())
        let u32 : uint32 = uint32(randomNumber())
        let u64 : uint64 = uint64(randomNumber())

        test i16
        test i32
        test i64
        test u16
        test u32
        test u64
    ()

[<Fact>]
let ``Single Double`` () =
    for i = 0 to randomCount do
        let single : single = single(random.NextDouble())
        let double : double = double(random.NextDouble())

        test single
        test double
    ()

[<Fact>]
let ``Bool Char Byte SByte Decimal`` () =
    for i = 0 to randomCount do
        let u8 : byte = byte(randomNumber())
        let i8 : sbyte = sbyte(randomNumber())
        let char : char = char(randomNumber())
        let bool : bool = int(randomNumber()) &&& 1 = 0
        let number : decimal = decimal(random.NextDouble())

        test u8
        test i8
        test char
        test bool
        test number
    ()

[<Fact>]
let ``Guid`` () =
    for i = 0 to randomCount do
        let guid : Guid = Guid.NewGuid()
        let bytes = generator.ToBytes<Guid> guid

        test guid

        let hex = guid.ToString("N")
        let items = [0..2..(hex.Length - 1)] |> Seq.map (fun x -> Convert.ToByte(hex.Substring(x, 2), 16))
        Assert.Equal(16, items |> Seq.length)
        if BitConverter.IsLittleEndian = Converter.UseLittleEndian then
            let array = guid.ToByteArray()
            Assert.Equal<byte>(array, bytes)
        if Converter.UseLittleEndian = false then
            Assert.Equal<byte>(items, bytes)
    ()

[<Fact>]
let ``TimeSpan Instance`` () =
    test (DateTime.Now - DateTime.MinValue)
    test (DateTime.MaxValue - DateTime.Now)
    test TimeSpan.MaxValue
    test TimeSpan.MinValue
    ()

[<Fact>]
let ``DateTime Instance`` () =
    let check item =
        test item
        let buffer = generator.ToBytes item
        let result = generator.ToValue<DateTime> buffer
        Assert.Equal(item, result)
        Assert.Equal(item.Kind, result.Kind)
        ()

    check DateTime.Now
    check DateTime.UtcNow
    check DateTime.MaxValue
    check DateTime.MinValue
    check (DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
    check (DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
    ()

[<Fact>]
let ``DateTimeOffset Instance`` () =
    let check item = 
        testExplicit item 10

        let buffer = generator.ToBytes item
        let result = generator.ToValue<DateTimeOffset> buffer
        let (origin, offset) = generator.ToValue<(int64 * int16)> buffer
        
        Assert.Equal(item, result)
        Assert.Equal(item.Offset, result.Offset)
        Assert.Equal(item.DateTime, result.DateTime)
        Assert.Equal(item.UtcDateTime, result.UtcDateTime)
        Assert.Equal(item.Ticks, result.Ticks)
        Assert.Equal(item.UtcTicks, result.UtcTicks)

        Assert.Equal(item.Ticks, origin)
        Assert.Equal(item.Offset.TotalMinutes |> int64, offset |> int64)
        ()

    check DateTimeOffset.Now
    check DateTimeOffset.UtcNow
    check DateTimeOffset.MaxValue
    check DateTimeOffset.MinValue
    check (new DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 180)))
    check (new DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -300)))
    check (new DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 30)))
    check (new DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -90)))
    ()

[<Fact>]
let ``Enum`` () =
    test DayOfWeek.Sunday
    test ConsoleColor.Cyan
    test ConsoleKey.Escape
    ()

[<Fact>]
let ``Enum Converter`` () =
    let value = generator.GetConverter typeof<DayOfWeek>
    Assert.StartsWith("UnsafePrimitiveConverter", value.GetType().Name)
    ()
