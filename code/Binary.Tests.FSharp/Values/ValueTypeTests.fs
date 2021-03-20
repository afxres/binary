module Values.ValueTypeTests

open Mikodev.Binary
open System
open System.Collections.Specialized
open Xunit

let random = Random()

let generator = Generator.CreateDefault()

let randomCount = 64

let RandomNumber () : uint64 =
    let buffer = Array.zeroCreate<byte>(sizeof<uint64>)
    random.NextBytes buffer
    let number = BitConverter.ToUInt64(buffer, 0)
    number

let TestWithSpan (value : 'a) (size : int) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.Encode(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(bufferOrigin, buffer)
    Assert.Equal(size, buffer.Length)

    let span = ReadOnlySpan buffer
    let result = converter.Decode &span
    Assert.Equal<'a>(value, result)
    ()

let TestWithBytes (value : 'a) (size : int) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let buffer = converter.Encode value
    Assert.Equal<byte>(bufferOrigin, buffer)
    Assert.Equal(size, buffer.Length)

    let result = converter.Decode buffer
    Assert.Equal<'a>(value, result)
    ()

let TestAuto (value : 'a) (size : int) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.EncodeAuto(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()
    Assert.Equal(size, buffer.Length)
    Assert.Equal<byte>(bufferOrigin, buffer)

    let mutable span = ReadOnlySpan buffer
    let result = converter.DecodeAuto &span
    Assert.True(span.IsEmpty)
    Assert.Equal<'a>(value, result)
    ()

let TestWithLengthPrefix (value : 'a) (size : int) =
    let bufferOrigin = generator.Encode value
    let converter = generator.GetConverter<'a>()

    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, value)
    let buffer = allocator.AsSpan().ToArray()

    let mutable span = ReadOnlySpan buffer
    let length = Converter.Decode &span
    Assert.Equal(size, length)
    Assert.Equal(size, span.Length)
    Assert.Equal<byte>(bufferOrigin, span.ToArray())

    let mutable span = ReadOnlySpan buffer
    let result = converter.DecodeWithLengthPrefix(&span)
    Assert.True(span.IsEmpty)
    Assert.Equal<'a>(value, result)
    ()

let TestExplicit (value : 'a) (size : int) =
    // convert via Generator
    let buffer = generator.Encode value
    Assert.Equal(size, buffer.Length)

    let result : 'a = generator.Decode buffer
    Assert.Equal<'a>(value, result)

    let converter = generator.GetConverter<'a>()
    Assert.Equal(size, converter.Length)

    // convert via Converter
    TestWithSpan value size
    // convert via bytes methods
    TestWithBytes value size
    // convert via 'auto' methods
    TestAuto value size
    // convert with length prefix
    TestWithLengthPrefix value size
    ()

let Test (value : 'a when 'a : unmanaged) = TestExplicit value sizeof<'a>

[<Fact>]
let ``Int & UInit 16, 32, 64`` () =
    for i = 1 to randomCount do
        let i16 : int16 = int16(RandomNumber())
        let i32 : int32 = int32(RandomNumber())
        let i64 : int64 = int64(RandomNumber())
        let u16 : uint16 = uint16(RandomNumber())
        let u32 : uint32 = uint32(RandomNumber())
        let u64 : uint64 = uint64(RandomNumber())

        Test i16
        Test i32
        Test i64
        Test u16
        Test u32
        Test u64
    ()

[<Fact>]
let ``Single Double`` () =
    for i = 0 to randomCount do
        let single : single = single(random.NextDouble())
        let double : double = double(random.NextDouble())

        Test single
        Test double
    ()

[<Fact>]
let ``Bool Char Byte SByte`` () =
    for i = 0 to randomCount do
        let u8 : byte = byte(RandomNumber())
        let i8 : sbyte = sbyte(RandomNumber())
        let char : char = char(RandomNumber())
        let bool : bool = int(RandomNumber()) &&& 1 = 0

        Test u8
        Test i8
        Test char
        Test bool
    ()

[<Fact>]
let ``Decimal`` () =
    for i = 0 to randomCount do
        let number : decimal = decimal(random.NextDouble())
        let converter = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "DecimalConverter") |> Array.exactlyOne |> Activator.CreateInstance :?> Converter<decimal>
        let alpha = converter.Encode number
        let bravo = Decimal.GetBits(number) |> Array.map (fun x -> BitConverter.GetBytes x) |> Array.concat
        Assert.Equal<byte>(alpha, bravo)
        Test number
    ()

[<Fact>]
let ``Guid`` () =
    for i = 0 to randomCount do
        let guid : Guid = Guid.NewGuid()
        let bytes = generator.Encode<Guid> guid

        Test guid

        let hex = guid.ToString("N")
        let items = [ 0..2..(hex.Length - 1) ] |> Seq.map (fun x -> Convert.ToByte(hex.Substring(x, 2), 16))
        Assert.Equal(16, items |> Seq.length)
        if BitConverter.IsLittleEndian then
            let array = guid.ToByteArray()
            Assert.Equal<byte>(array, bytes)
    ()

[<Fact>]
let ``TimeSpan Instance`` () =
    Test (DateTime.Now - DateTime.MinValue)
    Test (DateTime.MaxValue - DateTime.Now)
    Test TimeSpan.MaxValue
    Test TimeSpan.MinValue
    ()

[<Fact>]
let ``DateTime Instance`` () =
    let Ensure item =
        Test item
        let buffer = generator.Encode item
        let result = generator.Decode<DateTime> buffer
        Assert.Equal(item, result)
        Assert.Equal(item.Kind, result.Kind)
        ()

    Ensure DateTime.Now
    Ensure DateTime.UtcNow
    Ensure DateTime.MaxValue
    Ensure DateTime.MinValue
    Ensure (DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
    Ensure (DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
    ()

[<Fact>]
let ``DateTimeOffset Instance`` () =
    let Ensure item =
        TestExplicit item 10

        let buffer = generator.Encode item
        let result = generator.Decode<DateTimeOffset> buffer
        let (origin, offset) = generator.Decode<(int64 * int16)> buffer

        Assert.Equal(item, result)
        Assert.Equal(item.Offset, result.Offset)
        Assert.Equal(item.DateTime, result.DateTime)
        Assert.Equal(item.UtcDateTime, result.UtcDateTime)
        Assert.Equal(item.Ticks, result.Ticks)
        Assert.Equal(item.UtcTicks, result.UtcTicks)

        Assert.Equal(item.Ticks, origin)
        Assert.Equal(item.Offset.TotalMinutes |> int64, offset |> int64)
        ()

    Ensure DateTimeOffset.Now
    Ensure DateTimeOffset.UtcNow
    Ensure DateTimeOffset.MaxValue
    Ensure DateTimeOffset.MinValue
    Ensure (DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 180)))
    Ensure (DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -300)))
    Ensure (DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 30)))
    Ensure (DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -90)))
    ()

[<Fact>]
let ``DateTimeOffset Offset Ranged`` () =
    for i = -14 to 14 do
        let value = DateTimeOffset(DateTime(1970, 1, 1), TimeSpan.FromHours(float i))
        Assert.Equal(float i, value.Offset.TotalHours)
        TestExplicit value 10
    ()

[<Fact>]
let ``Enum`` () =
    Test DayOfWeek.Sunday
    Test ConsoleColor.Cyan
    Test ConsoleKey.Escape
    ()

[<Fact>]
let ``Enum Converter`` () =
    let value = generator.GetConverter typeof<DayOfWeek>
    Assert.StartsWith("NativeEndianConverter", value.GetType().Name)
    ()

[<Fact>]
let ``BitVector32`` () =
    for i = 0 to randomCount do
        Test (RandomNumber() |> int |> BitVector32)

    Test (BitVector32 0x11223344)
    Test (BitVector32 0xAABBCCDD)
    ()
