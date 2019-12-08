module Implementations.CollectionLayoutTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

let test<'a> (name : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal(name, converter.GetType().Name)

    // test internal builder name
    let builderField = converter.GetType().BaseType.GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let builder = builderField.GetValue(converter)
    Assert.Equal(builderName, builder.GetType().Name)

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

let testNull<'a when 'a : null> (name : string) (builderName : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()

    test name builderName collection

    // test null
    let delta = converter.Encode(null)
    let mutable allocator = Allocator()
    converter.EncodeWithLengthPrefix(&allocator, null)
    let hotel = allocator.AsSpan().ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

[<Fact>]
let ``Integration Test For Converter Implementations And Null Or Empty Collection Binary Layout`` () =
    test "ArrayLikeAdaptedConverter`2" "ArraySegmentBuilder`1" (ArraySegment<string>())
    test "ArrayLikeAdaptedConverter`2" "MemoryBuilder`1" (Memory<TimeSpan>())
    test "ArrayLikeAdaptedConverter`2" "ReadOnlyMemoryBuilder`1" (ReadOnlyMemory<int>())
    testNull "ArrayLikeAdaptedConverter`2" "ArrayBuilder`1" (Array.zeroCreate<int> 0)
    testNull "ArrayLikeAdaptedConverter`2" "ListDelegateBuilder`1" (ResizeArray<string>())

    testNull<IEnumerable<_>> "EnumerableAdaptedConverter`2" "IEnumerableBuilder`2" (ResizeArray<string>())
    testNull<IList<_>> "EnumerableAdaptedConverter`2" "IEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyList<_>> "EnumerableAdaptedConverter`2" "IEnumerableBuilder`2" (ResizeArray<string>())
    testNull<ICollection<_>> "EnumerableAdaptedConverter`2" "IEnumerableBuilder`2" (Array.zeroCreate<int> 0)
    testNull<IReadOnlyCollection<_>> "EnumerableAdaptedConverter`2" "IEnumerableBuilder`2" (Array.zeroCreate<int> 0)

    testNull<ISet<_>> "EnumerableAdaptedConverter`2" "ISetBuilder`2" (HashSet<TimeSpan>())
    testNull<HashSet<_>> "EnumerableAdaptedConverter`2" "ISetBuilder`2" (HashSet<int64>())
    testNull<HashSet<_>> "EnumerableAdaptedConverter`2" "ISetBuilder`2" (HashSet<string>())

    testNull<Dictionary<_, _>> "DictionaryAdaptedConverter`3" "IDictionaryBuilder`3" (Dictionary<int16, int64>())
    testNull<Dictionary<_, _>> "DictionaryAdaptedConverter`3" "IDictionaryBuilder`3" (Dictionary<string, int>())
    testNull<IDictionary<_, _>> "DictionaryAdaptedConverter`3" "IDictionaryBuilder`3" (Dictionary<int, string>())
    testNull<IReadOnlyDictionary<_, _>> "DictionaryAdaptedConverter`3" "IDictionaryBuilder`3" (Dictionary<string, int>())
    ()
