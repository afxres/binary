﻿module Contexts.AllocatorTests

open Mikodev.Binary
open System
open System.ComponentModel
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Xunit

let random = Random();

let generator = Generator.CreateDefault()

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
    AllocatorHelper.Append(&allocator, 256, null :> obj, fun a b -> ())
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
    let mutable allocator = Allocator()
    let span = ReadOnlySpan<byte>(source)
    AllocatorHelper.Append(&allocator, span)

    let span = allocator.AsSpan()
    Assert.Equal(span.Length, length)
    let result = span.ToArray()
    Assert.Equal<byte>(source, result)
    ()

[<Fact>]
let ``Get Pinnable Reference (default value)`` () =
    let allocator = Allocator()
    let result = &allocator.GetPinnableReference()
    let expect = &MemoryMarshal.GetReference(ReadOnlySpan())
    Assert.True(Unsafe.AreSame(&expect, &Unsafe.AsRef(&result)))
    Assert.True(Unsafe.AreSame(&expect, &MemoryMarshal.GetReference(allocator.AsSpan())))
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(1024)>]
[<InlineData(65536)>]
let ``Get Pinnable Reference (construct from byte array)`` (length : int) =
    let buffer = Array.zeroCreate<byte> length
    let allocator = Allocator(Span buffer)
    let result = &allocator.GetPinnableReference()
    let expect = &MemoryMarshal.GetReference(Span buffer)
    Assert.True(Unsafe.AreSame(&expect, &Unsafe.AsRef(&result)))
    Assert.True(Unsafe.AreSame(&expect, &MemoryMarshal.GetReference(allocator.AsSpan())))
    ()

[<Fact>]
let ``Get Pinnable Reference (attribute)`` () =
    let methodName = "GetPinnableReference"
    let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "Allocator") |> Array.exactlyOne
    let method = allocatorType.GetMethod(methodName)
    let methodOfSpan = typeof<Memory<byte>>.GetProperty("Span").PropertyType.GetMethod("GetPinnableReference")
    Assert.Equal(methodOfSpan.Name, method.Name)
    let attribute = method.GetCustomAttributes(typeof<EditorBrowsableAttribute>, false) |> Array.exactlyOne :?> EditorBrowsableAttribute
    Assert.Equal(EditorBrowsableState.Never, attribute.State)
    Assert.Equal(methodOfSpan.ReturnType, method.ReturnType)

    let attributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
    let methodReadOnlyAttributes = method.GetCustomAttributes(false) |> Array.filter (fun x -> x.GetType().FullName = attributeName)
    let returnReadOnlyAttributes = method.ReturnTypeCustomAttributes.GetCustomAttributes(false) |> Array.filter (fun x -> x.GetType().FullName = attributeName)
    Assert.Single methodReadOnlyAttributes |> ignore
    Assert.Single returnReadOnlyAttributes |> ignore
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
let ``To String (debug)`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 64), 32)
    AllocatorHelper.Append(&allocator, 4, null :> obj, fun a b -> ())
    Assert.Equal("Allocator(Length: 4, Capacity: 32, MaxCapacity: 32)", allocator.ToString())
    ()
