module Classes.AllocatorTests

open Mikodev.Binary
open System
open Xunit

let random = Random();

let generator = Generator()

[<Fact>]
let ``Allocate (zero)`` () =
    let allocator = new Allocator()
    let span = allocator.Allocate 0
    Assert.Equal(0, span.Length)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity);
    ()

[<Fact>]
let ``Allocate (for i++)`` () =
    let allocator = new Allocator()
    for item in 1..512 do
        let span = allocator.Allocate 1
        Assert.Equal(1, span.Length)
        Assert.Equal(item, allocator.Length)
#if DEBUG
        Assert.Equal(item, allocator.Capacity)
#else
        Assert.Equal((if item > 256 then 1024 else 256), allocator.Capacity)
#endif
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(256)>]
let ``Allocate (little, default constructor)`` (required : int) =
    let allocator = new Allocator()
    let span = allocator.Allocate required
    Assert.Equal(required, span.Length)
    Assert.Equal(required, allocator.Length)
#if DEBUG
    Assert.Equal(required, allocator.Capacity)
#else
    Assert.Equal(256, allocator.Capacity);
#endif
    ()

[<Theory>]
[<InlineData(257)>]
[<InlineData(512)>]
[<InlineData(1024)>]
let ``Allocate (normal)`` (required : int) =
    let allocator = new Allocator()
    let span = allocator.Allocate required
    Assert.Equal(required, span.Length)
    Assert.Equal(required, allocator.Length)
#if DEBUG
    Assert.Equal(required, allocator.Capacity)
#else
    Assert.Equal(1024, allocator.Capacity);
#endif
    ()

[<Fact>]
let ``Allocate (overflow, default constructor)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let allocator = new Allocator()
        let _ = allocator.Allocate(Int32.MaxValue + 1)
        ())
    Assert.Equal("Maximum allocator capacity has been reached.", error.Message)
    ()

[<Theory>]
[<InlineData(32)>]
[<InlineData(256)>]
[<InlineData(768)>]
let ``Allocate (overflow, limited)`` (limitation : int) =
    Assert.Throws<ArgumentException>(fun () ->
        let allocator = new Allocator(Array.empty, limitation)
        let _ = allocator.Allocate (limitation + 1)
        ()) |> ignore
    ()

[<Fact>]
let ``Allocate (limited)`` () =
    let allocator = new Allocator(Array.zeroCreate 96, 640)
    let _ = allocator.Allocate(192)
#if DEBUG
    Assert.Equal(192, allocator.Capacity)
#else
    Assert.Equal(96 <<< 2, allocator.Capacity)
#endif
    let _ = allocator.Allocate(448)
    Assert.Equal(640, allocator.Length)
    Assert.Equal(640, allocator.Capacity)
    ()

[<Fact>]
let ``Allocate (limited, zero)`` () =
    let allocator = new Allocator(Array.empty, 0)
    let span = allocator.Allocate 0
    Assert.Equal(0, span.Length)
    Assert.Equal(0, allocator.Capacity)
    Assert.Equal(0, allocator.Length)
    Assert.Throws<ArgumentException>(fun () ->
        let allocator = new Allocator(Array.empty, 0)
        let _ = allocator.Allocate 1
        ()) |> ignore
    ()

[<Fact>]
let ``Constructor (argument null)`` () =
    let alpha = new Allocator(Unchecked.defaultof<byte array>)
    let bravo = new Allocator(Unchecked.defaultof<byte array>, 256)
    Assert.Equal(0, alpha.Length)
    Assert.Equal(0, bravo.Length)
    Assert.Equal(0, alpha.Capacity)
    Assert.Equal(0, bravo.Capacity)
    Assert.Equal(Int32.MaxValue, alpha.MaxCapacity)
    Assert.Equal(256, bravo.MaxCapacity)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-255)>]
let ``Constructor (argument out of range)`` (limits : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let _ = new Allocator(Array.empty, limits)
        ())
    Assert.Equal("maxCapacity", error.ParamName)
    ()

[<Theory>]
[<InlineData(128, 127)>]
[<InlineData(32, 0)>]
let ``Constructor (buffer size greater than max capacity)`` (size : int, limits : int) =
    let allocator = new Allocator(Array.zeroCreate size, limits)
    Assert.Equal(limits, allocator.Capacity)
    Assert.Equal(limits, allocator.MaxCapacity)
    ()

[<Fact>]
let ``Constructor (default)`` () =
    let allocator = new Allocator()
    Assert.Equal(0, allocator.Length);
    Assert.Equal(0, allocator.Capacity);
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(255)>]
[<InlineData(4097)>]
let ``Constructor (byte array)`` (length : int) =
    let array = Array.zeroCreate<byte> length
    let allocator = new Allocator(array)
    Assert.Equal(length, allocator.Capacity)
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    let _ = allocator.Allocate 256
    Assert.Equal(allocator.Length, 256)
    ()

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(1, 1)>]
[<InlineData(128, 192)>]
let ``Constructor (limitation)`` (size : int, limitation : int) =
    let allocator = new Allocator(Array.zeroCreate size, limitation)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(size, allocator.Capacity)
    Assert.Equal(limitation, allocator.MaxCapacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(257)>]
let ``As Memory`` (length : int) =
    let source = Array.zeroCreate<byte> length
    let allocator = new Allocator()
    let span = new ReadOnlySpan<byte>(source)
    allocator.Append &span

    let memory = allocator.AsMemory()
    Assert.Equal(memory.Length, length)
    let result = memory.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(257)>]
let ``As Span`` (length : int) =
    let source = Array.zeroCreate<byte> length
    let allocator = new Allocator()
    let span = new ReadOnlySpan<byte>(source)
    allocator.Append &span

    let span = allocator.AsSpan()
    Assert.Equal(span.Length, length)
    let result = span.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``To Array (default value)`` () =
    let allocator = new Allocator()
    let result = allocator.ToArray()
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), result))
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(6144)>]
let ``To Array (buffer, empty)`` (size : int) =
    let buffer = Array.zeroCreate<byte> size
    let allocator = new Allocator(buffer)
    let result = allocator.ToArray()
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), result))
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(384)>]
let ``To Array (buffer)`` (size : int) =
    let buffer = [0..(size - 1)] |> List.map byte |> List.toArray
    let allocator = new Allocator(buffer)
    MemoryExtensions.CopyTo(buffer, allocator.Allocate(size))
    let result = allocator.ToArray()
    Assert.Equal<byte>(buffer, result)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let allocator = new Allocator(Array.zeroCreate 64, 32)
    let _ = allocator.Allocate(4)
    Assert.Equal("Allocator(Length: 4, Capacity: 32, MaxCapacity: 32)", allocator.ToString())
    ()
