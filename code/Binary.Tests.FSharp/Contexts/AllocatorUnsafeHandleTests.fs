module Contexts.AllocatorUnsafeHandleTests

open Mikodev.Binary
open System
open Xunit

[<Fact>]
let ``New Handle`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 24), 36)
    let handle = AllocatorUnsafeHandle &allocator
    let result = handle.AsAllocator()
    Assert.Equal(allocator.Capacity, result.Capacity)
    Assert.Equal(allocator.Length, result.Length)
    Assert.Equal(allocator.MaxCapacity, result.MaxCapacity)
    Assert.Equal(allocator.ToString(), result.ToString())
    ()

[<Fact>]
let ``Equals (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> AllocatorUnsafeHandle().Equals null |> ignore) |> ignore
    ()

[<Fact>]
let ``Get Hash Code (not supported)`` () =
    Assert.Throws<NotSupportedException>(fun () -> AllocatorUnsafeHandle().GetHashCode() |> ignore) |> ignore
    ()

[<Fact>]
let ``To String (debug)`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 24), 36)
    let handle = AllocatorUnsafeHandle &allocator
    Assert.Equal(allocator.ToString(), handle.ToString())
    ()

[<Fact>]
let ``To String (invalid default value)`` () =
    let handle = AllocatorUnsafeHandle()
    let message = "<Invalid Allocator Handle>"
    Assert.Equal(message, handle.ToString())
    ()
