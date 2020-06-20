module Implementations.EnumerableCounterTests

open Mikodev.Binary
open System.Collections.Generic
open System.Reflection
open Xunit

let generator = Generator.CreateDefault()

let testVariable<'a> () =
    let converter = generator.GetConverter<'a>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`2", converterType.Name)
    let encoderField = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let encoder = encoderField.GetValue(converter)
    let encoderType = encoder.GetType()
    Assert.Equal("SequenceVariableEncoder`2", encoderType.Name)
    ()

let testConstant<'a> (collection : 'a) (counterName : string) (count : int) =
    let converter = generator.GetConverter<'a>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`2", converterType.Name)
    let encoderField = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let encoder = encoderField.GetValue(converter)
    let encoderType = encoder.GetType()
    Assert.Equal("SequenceConstantEncoder`2", encoderType.Name)
    let counterField = encoderType.GetField("counter", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let counter = counterField.GetValue(encoder)
    Assert.NotNull counter
    let counterType = counter.GetType()
    Assert.Equal(counterName, counterType.Name)
    let counterInvokeMethodInfo = counterType.GetMethod("Invoke")
    let actualCount = counterInvokeMethodInfo.Invoke(counter, [| box collection |]) |> unbox<int>
    Assert.Equal(count, actualCount)
    ()

[<Fact>]
let ``Count Of IEnumerable<_>`` () =
    testVariable<IEnumerable<int>>()
    ()

[<Fact>]
let ``Count Of ICollection<_>``() =
    testConstant<ICollection<int>> ([| 1; 2; 4 |] |> HashSet<_>) "CollectionCounter`2" 3
    ()

[<Fact>]
let ``Count Of ICollection<_> (IDictionary<_, _>)``() =
    testConstant<IDictionary<int, int>> (Dictionary<int, int>()) "CollectionCounter`2" 0
    ()

[<Fact>]
let ``Count Of ICollection<_> (ISet<_>)``() =
    testConstant<ISet<int>> ([| 7; 8; 10 |] |> HashSet<_>) "CollectionCounter`2" 3
    ()

[<Fact>]
let ``Count Of IReadOnlyCollection<_>`` () =
    testConstant<IReadOnlyCollection<double>> ([| 2.0 |]) "ReadOnlyCollectionCounter`2" 1
    ()

[<Fact>]
let ``Count Of IReadOnlyCollection<_> (IReadOnlyDictionary<_, _>)`` () =
    testConstant<IReadOnlyDictionary<single, double>> ([| (single 1.1), 2.2 |] |> dict |> Dictionary<_, _>) "ReadOnlyCollectionCounter`2" 1
    ()

[<Fact>]
let ``Count Of Dictionary<_, _>`` () =
    testConstant<Dictionary<int, int>> ([| 3, 4; 7, 6; 9, 2 |] |> dict |> Dictionary<_, _>) "DictionaryCounter`2" 3
    ()

[<Fact>]
let ``Count Of LinkedList<_>`` () =
    testConstant<LinkedList<int>> ([ 5; 1 ] |> LinkedList<_>) "LinkedListCounter`1" 2
    ()

[<Fact>]
let ``Count Of HashSet<_>`` () =
    testConstant<HashSet<double>> ([ 0.6; 2.7; 3.1; 1.4 ] |> HashSet<_>) "HashSetCounter`1" 4
    ()
