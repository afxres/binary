module Classes.AllocatorUnsafeHandleTests

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
let ``Handle To String`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 24), 36)
    let handle = AllocatorUnsafeHandle &allocator
    Assert.Equal(allocator.ToString(), handle.ToString())
    ()

[<Fact>]
let ``Invalid Default Handle To String`` () =
    let handle = AllocatorUnsafeHandle()
    let message = "<Invalid Allocator Handle>"
    Assert.Equal(message, handle.ToString())
    ()
