module Values.ValueTypeTests

open Mikodev.Binary
open System
open System.Buffers.Binary
open System.Collections.Specialized
open System.Text
open Xunit

let generator = Generator.CreateDefault()

let randomCount = 64

let MakeConverters<'a> () =
    let converter = generator.GetConverter<'a>()
    [ converter ]

let TestWithSpan (converters: Converter<'a> list) (value: 'a) (size: int) =
    let bufferOrigin = generator.Encode value
    for converter in converters do
        let mutable allocator = Allocator()
        converter.Encode(&allocator, value)
        let buffer = allocator.AsSpan().ToArray()
        Assert.Equal<byte>(bufferOrigin, buffer)
        Assert.Equal(size, buffer.Length)

        let span = ReadOnlySpan buffer
        let result = converter.Decode &span
        Assert.Equal<'a>(value, result)
    ()

let TestWithBytes (converters: Converter<'a> list) (value: 'a) (size: int) =
    let bufferOrigin = generator.Encode value
    for converter in converters do
        let buffer = converter.Encode value
        Assert.Equal<byte>(bufferOrigin, buffer)
        Assert.Equal(size, buffer.Length)

        let result = converter.Decode buffer
        Assert.Equal<'a>(value, result)
    ()

let TestAuto (converters: Converter<'a> list) (value: 'a) (size: int) =
    let bufferOrigin = generator.Encode value
    for converter in converters do
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

let TestWithLengthPrefix (converters: Converter<'a> list) (value: 'a) (size: int) =
    let bufferOrigin = generator.Encode value
    for converter in converters do
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

let TestExplicit (value: 'a) (size: int) =
    // convert via Generator
    let buffer = generator.Encode value
    Assert.Equal(size, buffer.Length)

    let result: 'a = generator.Decode buffer
    Assert.Equal<'a>(value, result)

    let converters = MakeConverters<'a>()
    for converter in converters do
        Assert.Equal(size, converter.Length)

    // convert via Converter
    TestWithSpan converters value size
    // convert via bytes methods
    TestWithBytes converters value size
    // convert via 'auto' methods
    TestAuto converters value size
    // convert with length prefix
    TestWithLengthPrefix converters value size
    ()

let Test (value: 'a when 'a: unmanaged) = TestExplicit value sizeof<'a>

[<Fact>]
let ``Int & UInit 16, 32, 64`` () =
    for _ = 1 to randomCount do
        let i16 = int16 (Random.Shared.NextInt64())
        let i32 = int32 (Random.Shared.NextInt64())
        let i64 = int64 (Random.Shared.NextInt64())
        let u16 = uint16 (Random.Shared.NextInt64())
        let u32 = uint32 (Random.Shared.NextInt64())
        let u64 = uint64 (Random.Shared.NextInt64())

        Test i16
        Test i32
        Test i64
        Test u16
        Test u32
        Test u64
    ()

[<Fact>]
let ``Single Double`` () =
    for _ = 0 to randomCount do
        let single: single = single (Random.Shared.NextDouble())
        let double: double = double (Random.Shared.NextDouble())

        Test single
        Test double
    ()

[<Fact>]
let ``Bool Char Byte SByte`` () =
    for _ = 0 to randomCount do
        let u8: byte = byte (Random.Shared.NextInt64())
        let i8: sbyte = sbyte (Random.Shared.NextInt64())
        let char: char = char (Random.Shared.NextInt64())
        let bool: bool = int (Random.Shared.NextInt64()) &&& 1 = 0

        Test u8
        Test i8
        Test char
        Test bool
    ()

[<Fact>]
let ``Decimal`` () =
    for _ = 0 to randomCount do
        let number: decimal = decimal (Random.Shared.NextDouble())
        for converter in MakeConverters<decimal>() do
            let alpha = converter.Encode number
            let bravo =
                Decimal.GetBits(number)
                |> Array.map (fun x ->
                    let buffer = Array.zeroCreate<byte> sizeof<int>
                    BinaryPrimitives.WriteInt32LittleEndian(Span buffer, x)
                    buffer)
                |> Array.concat
            Assert.Equal<byte>(alpha, bravo)
            Test number
    ()

[<Fact>]
let ``Half`` () =
    for _ = 0 to randomCount do
        Test(Random.Shared.NextDouble() |> Half.op_Explicit)

    Test Half.MaxValue
    Test Half.MinValue
    ()

[<Fact>]
let ``BitVector32`` () =
    for _ = 0 to randomCount do
        Test(Random.Shared.Next() |> BitVector32)

    Test(BitVector32 0x11223344)
    Test(BitVector32 0xAABBCCDD)
    ()

[<Fact>]
let ``Guid`` () =
    for _ = 0 to randomCount do
        let guid: Guid = Guid.NewGuid()
        let bytes = generator.Encode<Guid> guid

        Test guid

        let hex = guid.ToString("N")
        let items = [ 0..2 .. (hex.Length - 1) ] |> Seq.map (fun x -> Convert.ToByte(hex.Substring(x, 2), 16))
        Assert.Equal(16, items |> Seq.length)
        if BitConverter.IsLittleEndian then
            let array = guid.ToByteArray()
            Assert.Equal<byte>(array, bytes)
    ()

[<Fact>]
let ``TimeSpan Instance`` () =
    Test(DateTime.Now - DateTime.MinValue)
    Test(DateTime.MaxValue - DateTime.Now)
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
    Ensure(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
    Ensure(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
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
    Ensure(DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 180)))
    Ensure(DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -300)))
    Ensure(DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float 30)))
    Ensure(DateTimeOffset(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeSpan.FromMinutes(float -90)))
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

[<Theory>]
[<InlineData(0x1F600)>]
[<InlineData(0x1F610)>]
let ``Rune`` (data: int) =
    let converters = MakeConverters<Rune>()
    for converter in converters do
        let rune = Rune data
        let buffer = converter.Encode rune
        let result = converter.Decode buffer
        Assert.Equal(data, result.Value)

        let bufferAuto = Allocator.Invoke(rune, fun allocator data -> converter.EncodeAuto(&allocator, data))
        Assert.Equal<byte>(buffer, bufferAuto)

        let bufferHead = Allocator.Invoke(rune, fun allocator data -> converter.EncodeWithLengthPrefix(&allocator, data))
        let mutable span = ReadOnlySpan bufferHead
        let body = Converter.DecodeWithLengthPrefix &span
        Assert.Equal(0, span.Length)
        Assert.Equal(4, body.Length)
        Assert.Equal<byte>(buffer, body.ToArray())
    ()

[<Theory>]
[<InlineData(0xD800)>]
[<InlineData(0xDFFF)>]
let ``Rune (decode invalid)`` (data: int) =
    let generator = Generator.CreateDefault()
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> generator.Decode<Rune>(generator.Encode data) |> ignore)
    Assert.StartsWith(ArgumentOutOfRangeException().Message, error.Message)
    ()

[<Fact>]
let ``DateOnly Instance`` () =
    for _ = 0 to randomCount do
        let number = Random.Shared.Next(DateOnly.MinValue.DayNumber, DateOnly.MaxValue.DayNumber + 1)
        let date = DateOnly.FromDayNumber number
        Test date

    Test DateOnly.MinValue
    Test DateOnly.MaxValue
    ()

[<Fact>]
let ``TimeOnly Instance`` () =
    for _ = 0 to randomCount do
        let number = Random.Shared.NextInt64(TimeOnly.MinValue.Ticks, TimeOnly.MaxValue.Ticks + 1L)
        let time = TimeOnly number
        Test time

    Test TimeOnly.MinValue
    Test TimeOnly.MaxValue
    ()
