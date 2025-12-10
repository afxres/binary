module Sequence.CollectionIntegrationTests

open Mikodev.Binary
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections.Immutable
open System.Reflection
open Xunit
open System.Collections

type TestConverter<'a>(length: int) =
    inherit Converter<'a>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'a = raise (NotSupportedException())

let generator = Generator.CreateDefault()

let TestEncode<'a> (converter: Converter<'a>) (collection: 'a) =
    let alpha = converter.Encode collection
    let bravo = Allocator.Invoke(collection, fun allocator item -> converter.Encode(&allocator, item))
    Assert.Empty alpha
    Assert.Empty bravo
    ()

let TestEncodeAutoAndEncodeWithLengthPrefix (converter: Converter<'a>) (collection: 'a) =
    let alpha = Allocator.Invoke(collection, fun allocator item -> converter.EncodeAuto(&allocator, item))
    let bravo = Allocator.Invoke(collection, fun allocator item -> converter.EncodeWithLengthPrefix(&allocator, item))
    Assert.Equal<byte>([| 0uy |], alpha)
    Assert.Equal<byte>([| 0uy |], bravo)
    ()

let TestDecode (converter: Converter<'a>) =
    let span = ReadOnlySpan<byte>()
    converter.Decode &span |> fun x -> Assert.IsType<'a>(x, exactMatch = false) |> ignore
    converter.Decode Array.empty |> fun x -> Assert.IsType<'a>(x, exactMatch = false) |> ignore
    ()

let TestDecodeAuto (converter: Converter<'a>) =
    let buffers = [| [| 0uy |]; [| 0x80uy; 0uy; 0uy; 0uy |] |]
    for i in buffers do
        let mutable span = ReadOnlySpan i
        converter.DecodeAuto &span |> fun x -> Assert.IsType<'a>(x, exactMatch = false) |> ignore
        Assert.True span.IsEmpty
        ()
    ()

let TestDecodeWithLengthPrefix (converter: Converter<'a>) =
    let buffers = [| [| 0uy |]; [| 0x80uy; 0uy; 0uy; 0uy |] |]
    for i in buffers do
        let mutable span = ReadOnlySpan i
        converter.DecodeWithLengthPrefix &span |> fun x -> Assert.IsType<'a>(x, exactMatch = false) |> ignore
        Assert.True span.IsEmpty
        ()
    ()

let rec TestFieldTypeName (instance: obj) (fieldName: string) (fieldTypeName: string) =
    let field = instance.GetType().GetField(fieldName, BindingFlags.Instance ||| BindingFlags.NonPublic)
    let fieldValue = field.GetValue(instance)
    let fieldActualTypeName = fieldValue.GetType().Name
    if (fieldTypeName.StartsWith "->") then
        Assert.Contains("Forward", fieldActualTypeName)
        TestFieldTypeName fieldValue fieldName (fieldTypeName.Substring 2)
    else
        Assert.Equal(fieldTypeName, fieldActualTypeName)
    ()

let TestSequence<'a when 'a: null> (encoderName: string) (decoderName: string) (collection: 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("SequenceConverter`1", converter.GetType().Name)

    // test internal builder name
    let encoder = converter.GetType().GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let encoderMethod = encoder.Method
    let encoderActualType = encoderMethod.DeclaringType
    let encoderActualName =
        if encoderMethod.Name.Contains("lambda") then
            "<lambda-encoder>"
        else
            encoderActualType.Name
    Assert.Equal(encoderName, encoderActualName)
    let decoder = converter.GetType().GetField("decode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    let decoderMethod = decoder.Method
    let decoderActualType = decoderMethod.DeclaringType
    let decoderActualName =
        if decoderMethod.Name.Contains("lambda") then
            "<lambda-decoder>"
        else
            decoderActualType.Name
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
let ``Collection Integration Test (collection, null or empty collection test, default interface implementation test)`` () =
    let bimodalDecoderName =
        if BitConverter.IsLittleEndian then
            "SpanLikeNativeEndianMethods"
        else
            "ListDecoder`1"
    TestSequence<IList<_>> "EnumerableEncoder`2" bimodalDecoderName (Array.zeroCreate<int> 0)
    TestSequence<ICollection<_>> "EnumerableEncoder`2" "ListDecoder`1" (Array.zeroCreate<TimeSpan> 0)
    TestSequence<IEnumerable<_>> "EnumerableEncoder`2" "ListDecoder`1" (Array.zeroCreate<string> 0)
    TestSequence<IReadOnlyList<_>> "EnumerableEncoder`2" bimodalDecoderName (ResizeArray<int>())
    TestSequence<IReadOnlyCollection<_>> "EnumerableEncoder`2" "ListDecoder`1" (ResizeArray<string>())

    TestSequence<Queue<_>> "<lambda-encoder>" "<lambda-decoder>" (Queue<int> 0)
    TestSequence<ImmutableList<_>> "<lambda-encoder>" "<lambda-decoder>" (ImmutableList.Create<string>())

    TestSequence<IImmutableList<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableList.Create<int>())
    TestSequence<IImmutableQueue<_>> "EnumerableEncoder`2" "<lambda-decoder>" (ImmutableQueue.Create<string>())

    TestSequence<HashSet<_>> "HashSetEncoder`1" "HashSetDecoder`1" (HashSet<int64>())
    TestSequence<HashSet<_>> "HashSetEncoder`1" "HashSetDecoder`1" (HashSet<string>())
    TestSequence<ISet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeSpan>())
    TestSequence<IReadOnlySet<_>> "EnumerableEncoder`2" "HashSetDecoder`1" (HashSet<TimeOnly>())
    ()

[<Fact>]
let ``Collection Integration Test (dictionary, null or empty collection test, default interface implementation test)`` () =
    TestSequence<Dictionary<_, _>> "DictionaryEncoder`2" "DictionaryDecoder`2" (Dictionary<int16, int64>())
    TestSequence<Dictionary<_, _>> "DictionaryEncoder`2" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<IDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<int, string>())
    TestSequence<IReadOnlyDictionary<_, _>> "KeyValueEnumerableEncoder`3" "DictionaryDecoder`2" (Dictionary<string, int>())
    TestSequence<SortedList<_, _>> "KeyValueEnumerableEncoder`3" "<lambda-decoder>" (SortedList<string, int>())
    TestSequence<SortedDictionary<_, _>> "<lambda-encoder>" "<lambda-decoder>" (SortedDictionary<TimeSpan, DateTime>())

    TestSequence<ConcurrentDictionary<_, _>> "KeyValueEnumerableEncoder`3" "<lambda-decoder>" (ConcurrentDictionary<TimeSpan, DateTime>())
    TestSequence<ImmutableDictionary<_, _>> "<lambda-encoder>" "<lambda-decoder>" (ImmutableDictionary.Create<TimeSpan, DateTime>())
    ()
