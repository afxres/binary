module Classes.AllocatorTests

open Mikodev.Binary
open System
open Xunit

let random = Random();

let generator = Generator.CreateDefault()

[<Fact>]
let ``Constructor (default)`` () =
    let allocator = new Allocator()
    Assert.Equal(0, allocator.Length);
    Assert.Equal(0, allocator.Capacity);
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-255)>]
let ``Constructor (argument out of range)`` (limits : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let _ = new Allocator(Span(), limits)
        ())
    Assert.Null(error.ParamName)
    Assert.Equal("Maximum allocator capacity must be greater than or equal to zero!", error.Message)
    ()

[<Theory>]
[<InlineData(128, 127)>]
[<InlineData(32, 0)>]
let ``Constructor (buffer size greater than max capacity)`` (size : int, limits : int) =
    let allocator = new Allocator(Span (Array.zeroCreate size), limits)
    Assert.Equal(limits, allocator.Capacity)
    Assert.Equal(limits, allocator.MaxCapacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(255)>]
[<InlineData(4097)>]
let ``Constructor (byte array)`` (length : int) =
    let array = Array.zeroCreate<byte> length
    let mutable allocator = new Allocator(Span array)
    Assert.Equal(length, allocator.Capacity)
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    AllocatorHelper.Append(&allocator, 256, null :> obj, fun a b -> ())
    Assert.Equal(allocator.Length, 256)
    ()

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(1, 1)>]
[<InlineData(128, 192)>]
let ``Constructor (limitation)`` (size : int, limitation : int) =
    let allocator = new Allocator(Span (Array.zeroCreate size), limitation)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(size, allocator.Capacity)
    Assert.Equal(limitation, allocator.MaxCapacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(257)>]
let ``As Span`` (length : int) =
    let source = Array.zeroCreate<byte> length
    let mutable allocator = new Allocator()
    let span = new ReadOnlySpan<byte>(source)
    AllocatorHelper.Append(&allocator, span)

    let span = allocator.AsSpan()
    Assert.Equal(span.Length, length)
    let result = span.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let mutable allocator = new Allocator(Span (Array.zeroCreate 64), 32)
    AllocatorHelper.Append(&allocator, 4, null :> obj, fun a b -> ())
    Assert.Equal("Allocator(Length: 4, Capacity: 32, MaxCapacity: 32)", allocator.ToString())
    ()
