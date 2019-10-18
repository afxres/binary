module Implementations.CollectionLayoutTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let generator = Generator()

type x () =

    static member ``Data Alpha`` : obj array array = [|
        [| "ArrayConverter`1"; Array.zeroCreate<int> 0; |];
        [| "ListConverter`1"; ResizeArray<string>(); |];
        [| "ArrayLikeConverter`1"; Memory<TimeSpan>(); |];
        [| "ArrayLikeConverter`1"; ReadOnlyMemory<int>(); |];
        [| "ArrayLikeConverter`1"; ArraySegment<string>(); |];
        [| "ISetConverter`1"; ArraySegment<string>(); |];
    |]

let test<'a> (name : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    Assert.Equal(name, converter.GetType().Name)
    let alpha = converter.ToBytes(collection)
    let mutable allocator = Allocator()
    converter.ToBytesWithLengthPrefix(&allocator, collection)
    let bravo = allocator.ToArray()

    Assert.Empty(alpha)
    Assert.Equal(0uy, Assert.Single(bravo))
    ()

let testNull<'a when 'a : null> (name : string) (collection : 'a) =
    let converter = generator.GetConverter<'a>()
    
    test name collection

    let delta = converter.ToBytes(null)
    let mutable allocator = Allocator()
    converter.ToBytesWithLengthPrefix(&allocator, null)
    let hotel = allocator.ToArray()

    Assert.Empty(delta)
    Assert.Equal(0uy, Assert.Single(hotel))
    ()

[<Fact>]
let ``Empty Or Null Collection Binary Layout`` () =
    test "ArrayLikeConverter`2" (ReadOnlyMemory<int>())
    test "ArrayLikeConverter`2" (ArraySegment<string>())
    test "ArrayLikeConverter`2" (Memory<TimeSpan>())
    testNull "ArrayConverter`1" (Array.zeroCreate<int> 0)
    testNull "ListConverter`1" (ResizeArray<string>())
    testNull "ISetConverter`2" (HashSet<TimeSpan>())
    testNull<ICollection<_>> "IEnumerableConverter`2" (Array.zeroCreate<int> 0)
    testNull<IEnumerable<_>> "IEnumerableConverter`2" (ResizeArray<string>())
    testNull<ISet<_>> "ISetConverter`2" (HashSet<TimeSpan>())
    ()
