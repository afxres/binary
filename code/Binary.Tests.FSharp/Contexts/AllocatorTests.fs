module Contexts.AllocatorTests

open Mikodev.Binary
open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
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
    let underlyingAllocator = { new IAllocator with member __.Allocate _ = raise (NotSupportedException()); &Unsafe.NullRef<byte>() };
    let a = Assert.Throws<ArgumentOutOfRangeException>(fun () -> let _ = Allocator(Span(), limits) in ())
    let b = Assert.Throws<ArgumentOutOfRangeException>(fun () -> let _ = Allocator(underlyingAllocator, limits) in ())
    let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let constructors = allocatorType.GetConstructors() |> Array.filter (fun x -> x.GetParameters().Length = 2)
    let parameterName = constructors |> Array.map (fun x -> x.GetParameters() |> Array.last) |> Array.map (fun x -> x.Name) |> Array.distinct |> Array.exactlyOne
    Assert.Equal("maxCapacity", parameterName)
    Assert.Equal("maxCapacity", a.ParamName)
    Assert.Equal("maxCapacity", b.ParamName)
    Assert.StartsWith("Argument max capacity must be greater than or equal to zero!", a.Message)
    Assert.StartsWith("Argument max capacity must be greater than or equal to zero!", b.Message)
    ()

[<Fact>]
let ``Constructor (argument underlying allocator null)`` () =
    let a = Assert.Throws<ArgumentNullException>(fun () -> let _ = Allocator(Unchecked.defaultof<IAllocator>) in ())
    let b = Assert.Throws<ArgumentNullException>(fun () -> let _ = Allocator(Unchecked.defaultof<IAllocator>, 100) in ())
    let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let constructors = allocatorType.GetConstructors() |> Array.filter (fun x -> x.GetParameters().[0].ParameterType = typeof<IAllocator>)
    let parameterName = constructors |> Array.map (fun x -> x.GetParameters() |> Array.head) |> Array.map (fun x -> x.Name) |> Array.distinct |> Array.exactlyOne
    Assert.Equal("underlyingAllocator", parameterName)
    Assert.Equal("underlyingAllocator", a.ParamName)
    Assert.Equal("underlyingAllocator", b.ParamName)
    Assert.StartsWith(ArgumentNullException().Message, a.Message)
    Assert.StartsWith(ArgumentNullException().Message, b.Message)
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

[<Fact>]
let ``As Span (default constructor)`` () =
    let allocator = Allocator()
    let span = allocator.AsSpan()
    Assert.Equal(0, span.Length)
    Assert.True(Unsafe.IsNullRef(&MemoryMarshal.GetReference(span)))
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
let ``To Array (default constructor)`` () =
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
    Assert.Equal("Length = 4, Capacity = 32, MaxCapacity = 32", allocator.ToString())
    ()
