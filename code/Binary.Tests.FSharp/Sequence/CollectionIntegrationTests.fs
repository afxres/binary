module Sequence.CollectionIntegrationTests

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
    let bravo = AllocatorHelper.Invoke(collection, fun allocator item -> converter.Encode(&allocator, item))
    Assert.Empty alpha
    Assert.Empty bravo
    ()

let TestEncodeAutoAndEncodeWithLengthPrefix (converter : Converter<'a>) (collection : 'a) =
    let alpha = AllocatorHelper.Invoke(collection, fun allocator item -> converter.EncodeAuto(&allocator, item))
    let bravo = AllocatorHelper.Invoke(collection, fun allocator item -> converter.EncodeWithLengthPrefix(&allocator, item))
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
    let encoderField = converter.GetType().GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let encoder = encoderField.GetValue converter
    Assert.Equal(encoderName, encoder.GetType().Name)
    let mutable decoderField = converter.GetType().GetField("decoder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let mutable decoder = decoderField.GetValue converter
    if typeof<'a>.IsInterface && typeof<'a>.Namespace <> "System.Collections.Immutable" then
        Assert.Equal("AssignableDecoder`2", decoder.GetType().Name)
    if typeof<'a>.IsInterface || decoder.GetType().Name = "DelegateDecoder`2" then
        decoderField <- decoder.GetType().GetField("decoder", BindingFlags.Instance ||| BindingFlags.NonPublic)
        decoder <- decoderField.GetValue decoder
    Assert.Equal(decoderName, decoder.GetType().Name)

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
    Test generator "SpanLikeVariableAdapter`1" "ArraySegmentBuilder`1" (ArraySegment<string>())
    Test generator "SpanLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<TimeSpan>())
    Test generator "SpanLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    TestNull "SpanLikeNativeEndianAdapter`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    TestNull "SpanLikeVariableAdapter`1" "ListBuilder`1" (ResizeArray<string>())
    ()

[<Fact>]
let ``Collection Integration Test (collection, null or empty collection test, default interface implementation test)`` () =
    TestSequence<IEnumerable<_>> "EnumerableEncoder`2" "ArraySegmentDecoder`1" (ResizeArray<string>())
    TestSequence<IList<_>> "EnumerableEncoder`2" "ArraySegmentDecoder`1" (Array.zeroCreate<int> 0)
    TestSequence<IReadOnlyList<_>> "EnumerableEncoder`2" "ArraySegmentDecoder`1" (ResizeArray<string>())
    TestSequence<ICollection<_>> "EnumerableEncoder`2" "ArraySegmentDecoder`1" (Array.zeroCreate<int> 0)
    TestSequence<IReadOnlyCollection<_>> "EnumerableEncoder`2" "ArraySegmentDecoder`1" (Array.zeroCreate<int> 0)

    TestSequence<Queue<_>> "DelegateEncoder`1" "EnumerableDecoder`1" (Queue<int> 0)
    TestSequence<ImmutableList<_>> "DelegateEncoder`1" "EnumerableDecoder`1" (ImmutableList.Create<string>())

    TestSequence<IImmutableList<_>> "EnumerableEncoder`2" "EnumerableDecoder`1" (ImmutableList.Create<int>())
    TestSequence<IImmutableQueue<_>> "EnumerableEncoder`2" "EnumerableDecoder`1" (ImmutableQueue.Create<string>())

    TestSequence<ISet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeSpan>())
    TestSequence<HashSet<_>> "DelegateEncoder`1" "HashSetDecoder`1" (HashSet<int64>())
    TestSequence<HashSet<_>> "DelegateEncoder`1" "HashSetDecoder`1" (HashSet<string>())
    TestSequence<LinkedList<_>> "LinkedListEncoder`1" "LinkedListDecoder`1" (LinkedList<double>())
    TestSequence<LinkedList<_>> "LinkedListEncoder`1" "LinkedListDecoder`1" (LinkedList<string>())
    ()

[<Fact>]
let ``Collection Integration Test (dictionary, null or empty collection test, default interface implementation test)`` () =
    TestSequence<Dictionary<_, _>> "DelegateEncoder`1" "DictionaryDecoder`2" (Dictionary<int16, int64>())
    TestSequence<Dictionary<_, _>> "DelegateEncoder`1" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<IDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<int, string>())
    TestSequence<IReadOnlyDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<SortedList<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (SortedList<string, int>())
    TestSequence<SortedDictionary<_, _>> "DelegateEncoder`1" "DictionaryDecoder`2" (SortedDictionary<TimeSpan, DateTime>())

    TestSequence<ConcurrentDictionary<_, _>> "KeyValueEnumerableEncoder`3" "KeyValueEnumerableDecoder`2" (ConcurrentDictionary<TimeSpan, DateTime>())
    TestSequence<ImmutableDictionary<_, _>> "DelegateEncoder`1" "KeyValueEnumerableDecoder`2" (ImmutableDictionary.Create<TimeSpan, DateTime>())
    ()

[<Fact>]
let ``Collection Integration Test (span-like collection)`` () =
    Test (Generator.CreateDefault()) "SpanLikeNativeEndianAdapter`1" "MemoryBuilder`1" (Memory<single>())
    Test (Generator.CreateDefaultBuilder().Build()) "SpanLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<double>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int32>(4)).Build()) "SpanLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<int32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int64>(8)).Build()) "SpanLikeConstantAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int64>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint32>(0)).Build()) "SpanLikeVariableAdapter`1" "MemoryBuilder`1" (Memory<uint32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint64>(0)).Build()) "SpanLikeVariableAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<uint64>())
    ()
