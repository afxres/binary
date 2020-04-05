module Implementations.CollectionLayoutTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

let test<'a> (generator : IGenerator) (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal("CollectionAdaptedConverter`3", converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let adapterField = converter.GetType().GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let builder = builderField.GetValue(converter)
    let adapter = adapterField.GetValue(converter)
    Assert.Equal(builderName, builder.GetType().Name)
    Assert.Equal(adapterName, adapter.GetType().Name)

    // test empty
    let alpha = converter.Encode(collection)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, collection)
    let bravo = allocator.AsSpan().ToArray()

    Assert.Empty(alpha)
    let mutable span = ReadOnlySpan bravo
    let length = PrimitiveHelper.DecodeNumber &span
    Assert.True(span.IsEmpty)
    Assert.Equal(0, length)
    ()

let testNull<'a when 'a : null> (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()

    test generator adapterName builderName collection

    // test null
    let delta = converter.Encode(null)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, null)
    let hotel = allocator.AsSpan().ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

[<Fact>]
let ``Collection Layout (integration test, adapter type test, builder type test, null or empty collection test, default interface implementation test)`` () =
    test generator "ArrayLikeVariableAdapter`1" "ArraySegmentBuilder`1" (ArraySegment<string>())
    test generator "ArrayLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<TimeSpan>())
    test generator "ArrayLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    testNull "ArrayLikeNativeEndianAdapter`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    testNull "ArrayLikeVariableAdapter`1" "ListBuilder`1" (ResizeArray<string>())

    testNull<IEnumerable<_>> "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (ResizeArray<string>())
    testNull<IList<_>> "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyList<_>> "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (ResizeArray<string>())
    testNull<ICollection<_>> "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyCollection<_>> "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<Queue<_>> "EnumerableAdapter`2" "DelegateEnumerableBuilder`2" (Queue<int> 0)

    testNull<ISet<_>> "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<TimeSpan>())
    testNull<HashSet<_>> "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<int64>())
    testNull<HashSet<_>> "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<string>())
    testNull<LinkedList<_>> "LinkedListAdapter`1" "FallbackEnumerableBuilder`2" (LinkedList<double>())
    testNull<LinkedList<_>> "LinkedListAdapter`1" "FallbackEnumerableBuilder`2" (LinkedList<string>())

    testNull<Dictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<int16, int64>())
    testNull<Dictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<string, int>())
    testNull<IDictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<int, string>())
    testNull<IReadOnlyDictionary<_, _>> "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<string, int>())
    testNull<SortedList<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (SortedList<string, int>())
    testNull<SortedDictionary<_, _>> "DictionaryAdapter`3" "DelegateEnumerableBuilder`2" (SortedDictionary<TimeSpan, DateTime>())
    ()

type TestConverter<'a> (length : int) =
    inherit Converter<'a>(length)

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_ : inref<ReadOnlySpan<byte>>) : 'a = raise (NotSupportedException())

[<Fact>]
let ``Array Like Collection Layout (integration test)`` () =
    test (Generator.CreateDefault()) "ArrayLikeNativeEndianAdapter`1" "MemoryBuilder`1" (Memory<single>())
    test (Generator.CreateDefaultBuilder().Build()) "ArrayLikeNativeEndianAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<double>())
    test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int32>(4)).Build()) "ArrayLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<int32>())
    test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<int64>(8)).Build()) "ArrayLikeConstantAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int64>())
    test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint32>(0)).Build()) "ArrayLikeVariableAdapter`1" "MemoryBuilder`1" (Memory<uint32>())
    test (Generator.CreateDefaultBuilder().AddConverter(TestConverter<uint64>(0)).Build()) "ArrayLikeVariableAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<uint64>())
    ()
