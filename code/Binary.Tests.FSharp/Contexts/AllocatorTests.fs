module Contexts.AllocatorTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``Constructor (default)`` () =
    let allocator = Allocator()
    Assert.Equal(0, allocator.Length);
    Assert.Equal(0, allocator.Capacity);
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-255)>]
let ``Constructor (argument out of range)`` (limits : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let _ = Allocator(Span(), limits)
        ())
    let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let constructor = allocatorType.GetConstructors() |> Array.filter (fun x -> x.GetParameters().Length = 2) |> Array.exactlyOne
    let parameter = constructor.GetParameters() |> Array.last
    Assert.Equal("maxCapacity", parameter.Name)
    Assert.Equal("maxCapacity", error.ParamName)
    Assert.StartsWith("Maximum capacity must be greater than or equal to zero!", error.Message)
    ()

[<Theory>]
[<InlineData(128, 127)>]
[<InlineData(32, 0)>]
let ``Constructor (buffer size greater than max capacity)`` (size : int, limits : int) =
    let allocator = Allocator(Span (Array.zeroCreate size), limits)
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
    let mutable allocator = Allocator(Span array)
    Assert.Equal(length, allocator.Capacity)
    Assert.Equal(Int32.MaxValue, allocator.MaxCapacity);
    Allocator.Append(&allocator, 256, null :> obj, fun a b -> ())
    Assert.Equal(allocator.Length, 256)
    ()

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(1, 1)>]
[<InlineData(128, 192)>]
let ``Constructor (limitation)`` (size : int, limitation : int) =
    let allocator = Allocator(Span (Array.zeroCreate size), limitation)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(size, allocator.Capacity)
    Assert.Equal(limitation, allocator.MaxCapacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(257)>]
let ``As Span`` (length : int) =
    let source = Array.zeroCreate<byte> length
    Random.Shared.NextBytes source
    let mutable allocator = Allocator()
    let span = ReadOnlySpan<byte>(source)
    Allocator.Append(&allocator, span)

    let span = allocator.AsSpan()
    Assert.Equal(span.Length, length)
    let result = span.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``To Array (empty)`` () =
    let mutable allocator = Allocator()
    let buffer = allocator.ToArray()
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), buffer))
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData(257)>]
let ``To Array`` (length : int) =
    let source = Array.zeroCreate<byte> length
    Random.Shared.NextBytes source
    let mutable allocator = Allocator()
    let span = ReadOnlySpan<byte>(source)
    Allocator.Append(&allocator, span)

    let result = allocator.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``Equals (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> Allocator().Equals null |> ignore) |> ignore
    ()

[<Fact>]
let ``Get Hash Code (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> Allocator().GetHashCode() |> ignore) |> ignore
    ()

[<Fact>]
let ``Equals (obsolete)`` () =
    let define = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let method = define.GetMethods() |> Array.filter (fun x -> x.Name = "Equals") |> Array.exactlyOne
    let attributes = method.GetCustomAttributes(typeof<ObsoleteAttribute>, false) |> Array.exactlyOne :?> ObsoleteAttribute
    let message = "Equals on Allocator will always throw an exception."
    Assert.Equal(message, attributes.Message)
    ()

[<Fact>]
let ``Get Hash Code (obsolete)`` () =
    let define = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let method = define.GetMethods() |> Array.filter (fun x -> x.Name = "GetHashCode") |> Array.exactlyOne
    let attributes = method.GetCustomAttributes(typeof<ObsoleteAttribute>, false) |> Array.exactlyOne :?> ObsoleteAttribute
    let message = "GetHashCode on Allocator will always throw an exception."
    Assert.Equal(message, attributes.Message)
    ()

[<Fact>]
let ``To String (debug)`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 64), 32)
    Allocator.Append(&allocator, 4, null :> obj, fun a b -> ())
    Assert.Equal("Allocator(Length: 4, Capacity: 32, MaxCapacity: 32)", allocator.ToString())
    ()
