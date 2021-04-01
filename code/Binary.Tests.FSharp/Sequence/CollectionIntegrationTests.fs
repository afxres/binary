﻿module Sequence.CollectionIntegrationTests

open Mikodev.Binary
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections.Immutable
open System.Reflection
open Xunit

type TestConverter<'a> (length : int) =
    inherit Converter<'a>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'a = raise (NotSupportedException())

let generator = Generator.CreateDefault()

let TestEncode<'a> (converter : Converter<'a>) (collection : 'a) =
    let alpha = converter.Encode collection
    let bravo = Allocator.Invoke(collection, fun allocator item -> converter.Encode(&allocator, item))
    Assert.Empty alpha
    Assert.Empty bravo
    ()

let TestEncodeAutoAndEncodeWithLengthPrefix (converter : Converter<'a>) (collection : 'a) =
    let alpha = Allocator.Invoke(collection, fun allocator item -> converter.EncodeAuto(&allocator, item))
    let bravo = Allocator.Invoke(collection, fun allocator item -> converter.EncodeWithLengthPrefix(&allocator, item))
    Assert.Equal<byte>([| 0uy |], alpha)
    Assert.Equal<byte>([| 0uy |], bravo)
    ()

let TestDecode (converter : Converter<'a>) =
    let span = ReadOnlySpan<byte>()
    converter.Decode &span |> Assert.IsAssignableFrom<'a> |> ignore
    converter.Decode Array.empty |> Assert.IsAssignableFrom<'a> |> ignore
    ()

let TestDecodeAuto (converter : Converter<'a>) =
    let buffers = [|
        [| 0uy |]
        [| 0x80uy; 0uy; 0uy; 0uy |]
    |]
    for i in buffers do
        let mutable span = ReadOnlySpan i
        converter.DecodeAuto &span |> Assert.IsAssignableFrom<'a> |> ignore
        Assert.True span.IsEmpty
        ()
    ()

let TestDecodeWithLengthPrefix (converter : Converter<'a>) =
    let buffers = [|
        [| 0uy |]
        [| 0x80uy; 0uy; 0uy; 0uy |]
    |]
    for i in buffers do
        let mutable span = ReadOnlySpan i
        converter.DecodeWithLengthPrefix &span |> Assert.IsAssignableFrom<'a> |> ignore
        Assert.True span.IsEmpty
        ()
    ()

let Test<'a> (generator : IGenerator) (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("SpanLikeConverter`2", converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let adapterField = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let builder = builderField.GetValue(converter)
    let adapter = adapterField.GetValue(converter)
    Assert.Equal(builderName, builder.GetType().Name)
    Assert.Equal(adapterName, adapter.GetType().Name)

    // test encode empty
    TestEncode converter collection
    TestEncodeAutoAndEncodeWithLengthPrefix converter collection

    // ensure can decode
    TestDecode converter
    TestDecodeAuto converter
    TestDecodeWithLengthPrefix converter
    ()

let TestNull<'a when 'a : null> (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()

    Test generator adapterName builderName collection

    // test null
    let delta = converter.Encode(null)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, null)
    let hotel = allocator.AsSpan().ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

let TestSequence<'a when 'a : null> (encoderName : string) (decoderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("SequenceConverter`1", converter.GetType().Name)

    // test internal builder name
    let encoder = converter.GetType().GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let encoderMethod = encoder.Method
    let encoderActualType = encoderMethod.DeclaringType
    let encoderActualName = if isNull (box encoderActualType) then "<lambda-encoder>" else encoderActualType.Name
    Assert.Equal(encoderName, encoderActualName)
    let decoder = converter.GetType().GetField("decoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let decoderMethod = decoder.Method
    let decoderActualType = decoderMethod.DeclaringType
    let decoderActualName = if isNull (box decoderActualType) then "<lambda-decoder>" else decoderActualType.Name
    Assert.Equal(decoderName, decoderActualName)

    // test encode empty
    TestEncode converter collection
    TestEncodeAutoAndEncodeWithLengthPrefix converter collection

    // ensure can decode
    TestDecode converter
    TestDecodeAuto converter
    TestDecodeWithLengthPrefix converter

    // test null
    let delta = converter.Encode(null)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, null)
    let hotel = allocator.AsSpan().ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

[<Fact>]
let ``Collection Integration Test (span-like collection, null or empty collection test)`` () =
    Test generator "VariableAdapter`1" "ArraySegmentBuilder`1" (ArraySegment<string>())
    Test generator "ConstantAdapter`1" "MemoryBuilder`1" (Memory<TimeSpan>())
    Test generator "NativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    TestNull "NativeEndianAdapter`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    TestNull "VariableAdapter`1" "ListBuilder`1" (ResizeArray<string>())
    ()

[<Fact>]
let ``Collection Integration Test (collection, null or empty collection test, default interface implementation test)`` () =
    TestSequence<IEnumerable<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (ResizeArray<string>())
    TestSequence<IList<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)
    TestSequence<IReadOnlyList<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (ResizeArray<string>())
    TestSequence<ICollection<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)
    TestSequence<IReadOnlyCollection<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)

    TestSequence<Queue<_>> "<lambda-encoder>" "<lambda-decoder>" (Queue<int> 0)
    TestSequence<ImmutableList<_>> "<lambda-encoder>" "<lambda-decoder>" (ImmutableList.Create<string>())

    TestSequence<IImmutableList<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableList.Create<int>())
    TestSequence<IImmutableQueue<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableQueue.Create<string>())

    TestSequence<ISet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeSpan>())
    TestSequence<HashSet<_>> "<lambda-encoder>" "HashSetDecoder`1" (HashSet<int64>())
    TestSequence<HashSet<_>> "<lambda-encoder>" "HashSetDecoder`1" (HashSet<string>())
    ()

[<Fact>]
let ``Collection Integration Test (dictionary, null or empty collection test, default interface implementation test)`` () =
    TestSequence<Dictionary<_, _>> "<lambda-encoder>" "DictionaryDecoder`2" (Dictionary<int16, int64>())
    TestSequence<Dictionary<_, _>> "<lambda-encoder>" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<IDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<int, string>())
    TestSequence<IReadOnlyDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<SortedList<_, _>> "KeyValueEnumerableEncoder`3" "<lambda-decoder>" (SortedList<string, int>())
    TestSequence<SortedDictionary<_, _>> "<lambda-encoder>" "<lambda-decoder>" (SortedDictionary<TimeSpan, DateTime>())

    TestSequence<ConcurrentDictionary<_, _>> "KeyValueEnumerableEncoder`3" "<lambda-decoder>" (ConcurrentDictionary<TimeSpan, DateTime>())
    TestSequence<ImmutableDictionary<_, _>> "<lambda-encoder>" "<lambda-decoder>" (ImmutableDictionary.Create<TimeSpan, DateTime>())
    ()

[<Fact>]
let ``Collection Integration Test (span-like collection)`` () =
    Test (Generator.CreateDefault()) "NativeEndianAdapter`1" "MemoryBuilder`1" (Memory<single>())
    Test (Generator.CreateDefaultBuilder().Build()) "NativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<double>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int32>(4)).Build()) "ConstantAdapter`1" "MemoryBuilder`1" (Memory<int32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int64>(8)).Build()) "ConstantAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int64>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint32>(0)).Build()) "VariableAdapter`1" "MemoryBuilder`1" (Memory<uint32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint64>(0)).Build()) "VariableAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<uint64>())
    ()
