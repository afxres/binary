module Implementations.CollectionLayoutTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

let test<'a> (name : string) (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal(name, converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().BaseType.GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let adapterField = converter.GetType().BaseType.GetField("adapter", BindingFlags.Instance ||| BindingFlags.NonPublic)
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

let testNull<'a when 'a : null> (name : string) (adapterName : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()

    test name adapterName builderName collection

    // test null
    let delta = converter.Encode(null)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, null)
    let hotel = allocator.AsSpan().ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

[<Fact>]
let ``Converter Implementations And Null Or Empty Collection Binary Layout (integration test)`` () =
    test "ArrayLikeAdaptedConverter`2" "ArrayLikeVariableAdapter`1" "ArraySegmentBuilder`1" (ArraySegment<string>())
    test "ArrayLikeAdaptedConverter`2" "ArrayLikeConstantAdapter`1" "MemoryBuilder`1" (Memory<TimeSpan>())
    test "ArrayLikeAdaptedConverter`2" "ArrayLikeOriginalEndiannessAdapter`1" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    testNull "ArrayLikeAdaptedConverter`2" "ArrayLikeOriginalEndiannessAdapter`1" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    testNull "ArrayLikeAdaptedConverter`2" "ArrayLikeVariableAdapter`1" "ListBuilder`1" (ResizeArray<string>())

    testNull<IEnumerable<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (ResizeArray<string>())
    testNull<IList<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyList<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (ResizeArray<string>())
    testNull<ICollection<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyCollection<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "FallbackEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<Queue<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "DelegateCollectionBuilder`2" (Queue<int> 0)
    testNull<Stack<_>> "CollectionAdaptedConverter`3" "EnumerableAdapter`2" "DelegateCollectionBuilder`2" (Stack<string> 0)

    testNull<ISet<_>> "CollectionAdaptedConverter`3" "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<TimeSpan>())
    testNull<HashSet<_>> "CollectionAdaptedConverter`3" "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<int64>())
    testNull<HashSet<_>> "CollectionAdaptedConverter`3" "SetAdapter`2" "FallbackEnumerableBuilder`2" (HashSet<string>())
    testNull<LinkedList<_>> "CollectionAdaptedConverter`3" "LinkedListAdapter`1" "FallbackEnumerableBuilder`2" (LinkedList<double>())
    testNull<LinkedList<_>> "CollectionAdaptedConverter`3" "LinkedListAdapter`1" "FallbackEnumerableBuilder`2" (LinkedList<string>())

    testNull<Dictionary<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<int16, int64>())
    testNull<Dictionary<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<string, int>())
    testNull<IDictionary<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<int, string>())
    testNull<IReadOnlyDictionary<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "FallbackEnumerableBuilder`2" (Dictionary<string, int>())
    testNull<SortedList<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "DelegateDictionaryBuilder`3" (SortedList<string, int>())
    testNull<SortedDictionary<_, _>> "CollectionAdaptedConverter`3" "DictionaryAdapter`3" "DelegateDictionaryBuilder`3" (SortedDictionary<TimeSpan, DateTime>())
    ()
