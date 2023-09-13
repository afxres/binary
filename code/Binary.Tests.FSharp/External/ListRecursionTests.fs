module External.ListRecursionTests

open Microsoft.FSharp.NativeInterop
open Mikodev.Binary
open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Xunit

#nowarn "9" // Uses of this construct may result in the generation of unverifiable .NET IL code
#nowarn "51" // The use of native pointers may result in unverifiable .NET IL code

[<StructLayout(LayoutKind.Explicit, Size = 1024)>]
type LargeBlock = struct end

type LargeBlockConverter() =
    inherit Converter<LargeBlock>(0)

    override __.Encode(allocator, item) =
        let mutable item = item
        Allocator.Append(&allocator, MemoryMarshal.CreateReadOnlySpan(&Unsafe.As<LargeBlock, byte>(&Unsafe.AsRef(&item)), Unsafe.SizeOf<LargeBlock>()))
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : LargeBlock =
        MemoryMarshal.AsRef<LargeBlock>(span)

[<Literal>]
let CommonLinuxStackSize = 8388608

[<Theory>]
[<InlineData(8192)>]
[<InlineData(32768)>]
let ``List Decode With Large Value Type`` (count : int) =
    let source = Array.zeroCreate<LargeBlock> count
    let span = MemoryMarshal.AsBytes(Span source)
    Random().NextBytes span

    Assert.True(span.Length >= CommonLinuxStackSize)

    let generator =
        Generator.CreateDefaultBuilder()
            .AddFSharpConverterCreators()
            .AddConverter(LargeBlockConverter())
            .Build()

    let list = List.ofArray source
    let converter = generator.GetConverter<LargeBlock list>()
    let buffer = converter.Encode list
    let result = converter.Decode buffer
    let target = List.toArray result
    let equals = MemoryExtensions.SequenceEqual(span, MemoryMarshal.AsBytes(ReadOnlySpan target))
    Assert.True equals
    ()

[<Theory>]
[<InlineData(8192)>]
[<InlineData(32768)>]
let ``List Decode Recursion With Large Value Type`` (count : int) =
    let rec DetectStackSize () =
        let mutable i = 0
        let address = &&i |> NativePtr.toVoidPtr |> IntPtr
        if not (RuntimeHelpers.TryEnsureSufficientExecutionStack()) then
            address |> List.singleton
        else
            address :: DetectStackSize ()

    let Detect () =
        let list = DetectStackSize ()
        let head = list |> List.head
        let last = list |> List.last
        head - last

    let length = Detect ()
    let buffer = NativePtr.stackalloc<byte> (int length + 1024)
    Assert.NotEqual(IntPtr.Zero, buffer |> NativePtr.toVoidPtr |> IntPtr)
    Assert.False(RuntimeHelpers.TryEnsureSufficientExecutionStack())

    ``List Decode With Large Value Type`` count
    ()
