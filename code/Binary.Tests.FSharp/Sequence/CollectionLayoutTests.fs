module Sequence.CollectionLayoutTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

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
        [| 0x40uy; 0uy |]
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
        [| 0x40uy; 0uy |]
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
    let converterName = if adapterName.StartsWith("SpanLike") then "SpanLikeConverter`2" else "SequenceConverter`2"
    Assert.Equal(converterName, converter.GetType().Name)

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

[<Fact>]
let ``Collection Layout Integration Test (adapter type test, builder type test, null or empty collection test, default interface implementation test)`` () =
    Test generator "SpanLikeVariableAdapter`1" "ArraySegmentBuilder`1" (ArraySegment<string>())
    Test generator "SpanLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<TimeSpan>())
    Test generator "SpanLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    TestNull "SpanLikeNativeEndianAdapter`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    TestNull "SpanLikeVariableAdapter`1" "ListBuilder`1" (ResizeArray<string>())

    TestNull<IEnumerable<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (ResizeArray<string>())
    TestNull<IList<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    TestNull<IReadOnlyList<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (ResizeArray<string>())
    TestNull<ICollection<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    TestNull<IReadOnlyCollection<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    TestNull<Queue<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (Queue<int> 0)

    TestNull<ISet<_>> "SetAdapter`2" "DelegateEnumerableBuilder`2" (HashSet<TimeSpan>())
    TestNull<HashSet<_>> "SetAdapter`2" "FallbackEnumerableBuilder`1" (HashSet<int64>())
    TestNull<HashSet<_>> "SetAdapter`2" "FallbackEnumerableBuilder`1" (HashSet<string>())
    TestNull<LinkedList<_>> "LinkedListAdapter`1" "FallbackEnumerableBuilder`1" (LinkedList<double>())
    TestNull<LinkedList<_>> "LinkedListAdapter`1" "FallbackEnumerableBuilder`1" (LinkedList<string>())

    TestNull<Dictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`1" (Dictionary<int16, int64>())
    TestNull<Dictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`1" (Dictionary<string, int>())
    TestNull<IDictionary<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (Dictionary<int, string>())
    TestNull<IReadOnlyDictionary<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (Dictionary<string, int>())
    TestNull<SortedList<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (SortedList<string, int>())
    TestNull<SortedDictionary<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (SortedDictionary<TimeSpan, DateTime>())
    ()

type TestConverter<'a> (length : int) =
    inherit Converter<'a>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'a = raise (NotSupportedException())

[<Fact>]
let ``Collection Layout Integration Test (span-like collection)`` () =
    Test (Generator.CreateDefault()) "SpanLikeNativeEndianAdapter`1" "MemoryBuilder`1" (Memory<single>())
    Test (Generator.CreateDefaultBuilder().Build()) "SpanLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<double>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int32>(4)).Build()) "SpanLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<int32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int64>(8)).Build()) "SpanLikeConstantAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int64>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint32>(0)).Build()) "SpanLikeVariableAdapter`1" "MemoryBuilder`1" (Memory<uint32>())
    Test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint64>(0)).Build()) "SpanLikeVariableAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<uint64>())
    ()
