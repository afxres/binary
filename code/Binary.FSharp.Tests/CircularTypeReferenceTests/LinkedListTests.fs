module CircularTypeReferenceTests.LinkedListTests

open Microsoft.FSharp.NativeInterop
open Mikodev.Binary
open System
open System.Runtime.CompilerServices
open Xunit

type LinkedList<'T> =
    | Null
    | Item of 'T * LinkedList<'T>

[<Fact>]
let ``Custom Linked List Encode Large Data Test`` () =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<LinkedList<byte>>()
    Assert.Equal("UnionConverter`1", converter.GetType().Name)

    let mutable head = LinkedList<byte>.Null
    for i = 1 to 0x100_000 do
        head <- Item(byte i, head)
        ()
    Assert.Throws<InsufficientExecutionStackException>(fun () -> converter.Encode head |> ignore) |> ignore
    ()

[<Fact>]
let ``Custom Linked List Decode Large Data Test`` () =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<LinkedList<byte>>()
    Assert.Equal("UnionConverter`1", converter.GetType().Name)

    let buffer = Array.create 0x100_000 1uy
    Assert.Throws<InsufficientExecutionStackException>(fun () -> converter.Decode buffer |> ignore) |> ignore
    ()

#nowarn "9" // Uses of this construct may result in the generation of unverifiable .NET IL code

let rec InvokeWithInsufficientExecutionStack (action: unit -> 'T) =
    let buffer = NativePtr.stackalloc<byte> 1024
    NativePtr.set buffer 0 1uy
    NativePtr.get buffer 0 |> ignore
    if RuntimeHelpers.TryEnsureSufficientExecutionStack() then
        InvokeWithInsufficientExecutionStack action
    else
        action ()

[<Fact>]
let ``Custom Linked List All Methods With Insufficient Execution Stack Test`` () =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<LinkedList<byte>>()
    Assert.Equal("UnionConverter`1", converter.GetType().Name)

    let source = LinkedList<byte>.Item(2uy, LinkedList<byte>.Null)
    let buffer = converter.Encode source
    let bufferAuto = Allocator.Invoke(source, fun allocator source -> converter.EncodeAuto(&allocator, source))
    Assert.Throws<InsufficientExecutionStackException>(fun () ->
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable allocator = Allocator()
            converter.Encode(&allocator, source)
            allocator.Length)
        |> ignore)
    |> ignore
    Assert.Throws<InsufficientExecutionStackException>(fun () ->
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable allocator = Allocator()
            converter.EncodeAuto(&allocator, source)
            allocator.Length)
        |> ignore)
    |> ignore
    Assert.Throws<InsufficientExecutionStackException>(fun () ->
        InvokeWithInsufficientExecutionStack(fun () ->
            let span = ReadOnlySpan buffer
            converter.Decode &span)
        |> ignore)
    |> ignore
    Assert.Throws<InsufficientExecutionStackException>(fun () ->
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable span = ReadOnlySpan bufferAuto
            converter.DecodeAuto &span)
        |> ignore)
    |> ignore
    ()

type ControlGroupUnionObject<'T> =
    | Null
    | Data of 'T

let ``Control Group All Methods With Insufficient Execution Stack Test`` () =
    let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
    let converter = generator.GetConverter<ControlGroupUnionObject<byte>>()
    Assert.Equal("UnionConverter`1", converter.GetType().Name)

    let source = ControlGroupUnionObject<byte>.Data 4uy
    let buffer =
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable allocator = Allocator()
            converter.Encode(&allocator, source)
            allocator.ToArray())
    let bufferAuto =
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable allocator = Allocator()
            converter.EncodeAuto(&allocator, source)
            allocator.ToArray())
    let result =
        InvokeWithInsufficientExecutionStack(fun () ->
            let span = ReadOnlySpan buffer
            converter.Decode &span)
    let resultAuto =
        InvokeWithInsufficientExecutionStack(fun () ->
            let mutable span = ReadOnlySpan buffer
            converter.DecodeAuto &span)
    Assert.Equal(source, result)
    Assert.Equal(source, resultAuto)
    ()
