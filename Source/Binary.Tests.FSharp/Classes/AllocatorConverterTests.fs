module Classes.AllocatorConverterTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Runtime.InteropServices
open Xunit

type ArrayWrapper<'T> = { array : 'T array }

type ArrayWrapperConverter<'T when 'T : struct and 'T :> ValueType and 'T : (new : unit -> 'T)> () =
    inherit Converter<ArrayWrapper<'T>> ()

    override __.Encode(allocator, item) =
        let span = MemoryMarshal.Cast<'T, byte>(ReadOnlySpan item.array)
        AllocatorHelper.Append(&allocator, span)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : ArrayWrapper<'T> =
        let span = MemoryMarshal.Cast<byte, 'T> span
        { array = span.ToArray() }

let random = Random()

[<Fact>]
let ``Variable Converter Length Prefix`` () =
    let converter = ArrayWrapperConverter<byte>()
    for i = 0 to 128 do
        let source = { array = Array.zeroCreate<byte> i }
        random.NextBytes source.array
        let mutable allocator = Allocator()
        converter.EncodeWithLengthPrefix(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()

        if i < 32 then
            Assert.Equal(i, int buffer.[0])
            Assert.Equal(i + 1, buffer.Length)
        else
            Assert.Equal(i + 4, buffer.Length)

        let mutable span = ReadOnlySpan buffer
        let result = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        Assert.Equal(0, span.Length)
        Assert.Equal<byte>(source.array, result.ToArray())
    ()

[<Fact>]
let ``Uncountable Collection Length Prefix`` () =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<IEnumerable<byte>>()
    for i = 0 to 128 do
        let source = Array.zeroCreate<byte> i
        random.NextBytes source
        let source = source |> Array.toSeq

        let mutable allocator = Allocator()
        converter.EncodeWithLengthPrefix(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()

        if i < 32 then
            Assert.Equal(i, int buffer.[0])
            Assert.Equal(i + 1, buffer.Length)
        else
            Assert.Equal(i + 4, buffer.Length)

        let mutable span = ReadOnlySpan buffer
        let result = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        Assert.Equal(0, span.Length)
        Assert.Equal<byte>(source, result.ToArray())
    ()

[<Fact>]
let ``Allocator Anchor Length Prefix`` () =
    for i = 0 to 128 do
        let source = Array.zeroCreate<byte> i
        random.NextBytes source

        let mutable allocator = Allocator()
        let anchor = AllocatorHelper.AnchorLengthPrefix &allocator
        AllocatorHelper.Append(&allocator, ReadOnlySpan source)
        AllocatorHelper.AppendLengthPrefix(&allocator, anchor)
        let buffer = allocator.AsSpan().ToArray()

        Assert.Equal(i + 4, buffer.Length)

        let mutable span = ReadOnlySpan buffer
        let result = PrimitiveHelper.DecodeBufferWithLengthPrefix &span
        Assert.Equal(0, span.Length)
        Assert.Equal<byte>(source, result.ToArray())
    ()
