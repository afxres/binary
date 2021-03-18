module External.ListRecursionTests

open Mikodev.Binary
open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Xunit

#nowarn "9" // Uses of this construct may result in the generation of unverifiable .NET IL code.

[<StructLayout(LayoutKind.Explicit, Size = 1024)>]
type LargeBlock = struct end

type LargeBlockConverter() =
    inherit Converter<LargeBlock>(0)

    override __.Encode(allocator, item) =
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
