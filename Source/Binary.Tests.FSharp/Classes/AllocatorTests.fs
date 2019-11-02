module Classes.AllocatorTests

open Mikodev.Binary
open System
open Xunit

let random = Random();

let generator = Generator.CreateDefault()

[<Fact>]
let ``Allocate (default constructor, zero)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = new Allocator() in
        let _ = AllocatorHelper.Allocate(&allocator, 0)
        ())
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate Reference (default constructor, zero)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = new Allocator() in
        let _ = AllocatorHelper.AllocateReference(&allocator, 0)
        ())
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate (default constructor, overflow)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = new Allocator() in
        let _ = AllocatorHelper.Allocate(&allocator, Int32.MaxValue + 1)
        ())
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate (limited to zero, zero)`` () =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = new Allocator(Array.empty, 0) in
        let _ = AllocatorHelper.Allocate(&allocator, 0)
        ())
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate Some Then Allocate Zero`` () =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator()
        let _ = AllocatorHelper.Allocate(&allocator, 32)
        flag <- 1
        let _ = AllocatorHelper.Allocate(&allocator, 0)
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate Some Then Allocate Reference Zero`` () =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator()
        let _ = AllocatorHelper.Allocate(&allocator, 32)
        flag <- 1
        let _ = AllocatorHelper.AllocateReference(&allocator, 0)
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument length must be greater than zero!" + Environment.NewLine, error.Message)
    Assert.Equal("length", error.ParamName)
    let methodInfo = typeof<AllocatorHelper>.GetMethod("Allocate")
    let parameterName = methodInfo.GetParameters() |> Array.last |> (fun x -> x.Name)
    Assert.Equal(parameterName, error.ParamName)
    ()

[<Fact>]
let ``Allocate (for 1 to 512)`` () =
    let mutable allocator = new Allocator()
    for item in 1..512 do
        let span = AllocatorHelper.Allocate(&allocator, 1)
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
    let mutable allocator = new Allocator()
    let span = AllocatorHelper.Allocate(&allocator, required)
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
    let mutable allocator = new Allocator()
    let span = AllocatorHelper.Allocate(&allocator, required)
    Assert.Equal(required, span.Length)
    Assert.Equal(required, allocator.Length)
#if DEBUG
    Assert.Equal(required, allocator.Capacity)
#else
    Assert.Equal(1024, allocator.Capacity);
#endif
    ()

[<Theory>]
[<InlineData(32)>]
[<InlineData(256)>]
[<InlineData(768)>]
let ``Allocate (overflow, limited)`` (limitation : int) =
    Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = new Allocator(Array.empty, limitation)
        let _ = AllocatorHelper.Allocate(&allocator, limitation + 1)
        ()) |> ignore
    ()

[<Fact>]
let ``Allocate (limited)`` () =
    let mutable allocator = new Allocator(Array.zeroCreate 96, 640)
    let _ = AllocatorHelper.Allocate(&allocator, 192)
#if DEBUG
    Assert.Equal(192, allocator.Capacity)
#else
    Assert.Equal(96 <<< 2, allocator.Capacity)
#endif
    let _ = AllocatorHelper.Allocate(&allocator, 448)
    Assert.Equal(640, allocator.Length)
    Assert.Equal(640, allocator.Capacity)
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
    let error = Assert.Throws<ArgumentException>(fun () ->
        let _ = new Allocator(Array.empty, limits)
        ())
    Assert.Null(error.ParamName)
    Assert.Equal("Maximum allocator capacity must be greater than or equal to zero!", error.Message)
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
    let mutable allocator = new Allocator(array)
    Assert.Equal(length, allocator.Capacity)
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    let _ = AllocatorHelper.Allocate(&allocator, 256)
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
    let mutable allocator = new Allocator()
    let span = new ReadOnlySpan<byte>(source)
    AllocatorHelper.Append(&allocator, &span)

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
    let mutable allocator = new Allocator()
    let span = new ReadOnlySpan<byte>(source)
    AllocatorHelper.Append(&allocator, &span)

    let span = allocator.AsSpan()
    Assert.Equal(span.Length, length)
    let result = span.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``To Array (default value)`` () =
    let allocator = new Allocator()
    let result = allocator.AsSpan().ToArray()
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), result))
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(6144)>]
let ``To Array (buffer, empty)`` (size : int) =
    let buffer = Array.zeroCreate<byte> size
    let allocator = new Allocator(buffer)
    let result = allocator.AsSpan().ToArray()
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), result))
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(384)>]
let ``To Array (buffer)`` (size : int) =
    let buffer = [0..(size - 1)] |> List.map byte |> List.toArray
    let mutable allocator = new Allocator(buffer)
    MemoryExtensions.CopyTo(buffer, AllocatorHelper.Allocate(&allocator, size))
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(buffer, result)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let mutable allocator = new Allocator(Array.zeroCreate 64, 32)
    let _ = AllocatorHelper.Allocate(&allocator, 4)
    Assert.Equal("Allocator(Length: 4, Capacity: 32, MaxCapacity: 32)", allocator.ToString())
    ()
