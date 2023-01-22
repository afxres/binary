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

let TestFieldTypeName<'a> (converter : Converter<'a>) (fieldName : string) (fieldTypeName : string) =
    let field = converter.GetType().GetField(fieldName, BindingFlags.Instance ||| BindingFlags.NonPublic)
    let fieldValue = field.GetValue(converter)
    Assert.Equal(fieldTypeName, if isNull fieldValue then null else fieldValue.GetType().Name)
    ()

let Test<'a> (generator : IGenerator) (encoderName : string) (decoderName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("SpanLikeConverter`2", converter.GetType().Name)

    // test internal field name
    TestFieldTypeName<'a> converter "builder" builderName
    TestFieldTypeName<'a> converter "encoder" encoderName
    TestFieldTypeName<'a> converter "decoder" decoderName

    // test encode empty
    TestEncode converter collection
    TestEncodeAutoAndEncodeWithLengthPrefix converter collection

    // ensure can decode
    TestDecode converter
    TestDecodeAuto converter
    TestDecodeWithLengthPrefix converter
    ()

let TestNull<'a when 'a : null> (encoderName : string) (decoderName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()

    Test generator encoderName decoderName builderName collection

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
    let encoder = converter.GetType().GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let encoderMethod = encoder.Method
    let encoderActualType = encoderMethod.DeclaringType
    let encoderActualName = if isNull (box encoderActualType) then "<lambda-encoder>" else encoderActualType.Name
    Assert.Equal(encoderName, encoderActualName)
    let decoder = converter.GetType().GetField("decode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
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
    Test generator "FallbackConstantEncoder`1" null "ArraySegmentBuilder`1" (ArraySegment<struct (int * int64)>())
    Test generator "FallbackVariableEncoder`1" null "ArraySegmentBuilder`1" (ArraySegment<string>())
    Test generator "ConstantEncoder`2" "ConstantDecoder`2" "MemoryBuilder`1" (Memory<TimeSpan>())
    Test generator "NativeEndianEncoder`1" "NativeEndianDecoder`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    Test generator "NativeEndianEncoder`1" "NativeEndianDecoder`1" "ImmutableArrayBuilder`1" (ImmutableArray<double>())
    TestNull "FallbackConstantEncoder`1" null "ArrayBuilder`1" (Array.zeroCreate<struct (int16 * int64)> 0)
    TestNull "FallbackVariableEncoder`1" null "ListBuilder`1" (ResizeArray<string>())
    TestNull "ConstantEncoder`2" "ConstantDecoder`2" "ListBuilder`1" (ResizeArray<DateTime>())
    TestNull "NativeEndianEncoder`1" "NativeEndianDecoder`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    ()

[<Fact>]
let ``Collection Integration Test (collection, null or empty collection test, default interface implementation test)`` () =
    TestSequence<IList<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)
    TestSequence<ICollection<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)
    TestSequence<IEnumerable<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (ResizeArray<string>())
    TestSequence<IReadOnlyList<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (ResizeArray<string>())
    TestSequence<IReadOnlyCollection<_>> "EnumerableEncoder`2" "EnumerableDecoder`2" (Array.zeroCreate<int> 0)

    TestSequence<Queue<_>> "<lambda-encoder>" "<lambda-decoder>" (Queue<int> 0)
    TestSequence<ImmutableList<_>> "<lambda-encoder>" "<lambda-decoder>" (ImmutableList.Create<string>())

    TestSequence<IImmutableList<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableList.Create<int>())
    TestSequence<IImmutableQueue<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableQueue.Create<string>())

    TestSequence<HashSet<_>> "<lambda-encoder>" "HashSetDecoder`1" (HashSet<int64>())
    TestSequence<HashSet<_>> "<lambda-encoder>" "HashSetDecoder`1" (HashSet<string>())
    TestSequence<ISet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeSpan>())
    TestSequence<IReadOnlySet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeOnly>())
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
let ``Collection Integration Test (span-like collection, custom converter)`` () =
    Test (Generator.CreateDefault()) "NativeEndianEncoder`1" "NativeEndianDecoder`1" "MemoryBuilder`1" (Memory<single>())
    Test (Generator.CreateDefaultBuilder().Build()) "NativeEndianEncoder`1" "NativeEndianDecoder`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<double>())
    Test (Generator.CreateDefault()) "ConstantEncoder`2" "ConstantDecoder`2" "ArraySegmentBuilder`1" (ArraySegment<DateTime>())
    Test (Generator.CreateDefaultBuilder().Build()) "ConstantEncoder`2" "ConstantDecoder`2" "ImmutableArrayBuilder`1" (ImmutableArray<TimeOnly>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int64>(8)).Build()) "FallbackConstantEncoder`1" null "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int64>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint64>(0)).Build()) "FallbackVariableEncoder`1" null "ImmutableArrayBuilder`1" (ImmutableArray<uint64>())
    ()
