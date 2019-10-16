module Creators.DictionaryTests

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

let generator = new Generator()

[<Fact>]
let ``Dictionary`` () =
    let a = new Dictionary<int, double>()
    a.Add(1, 1.1)
    a.Add(2, 2.2)
    let b = new Dictionary<string, Guid>()
    b.Add("one", Guid.NewGuid())
    b.Add("zero", Guid.NewGuid())
    let bytesA = generator.ToBytes a
    let bytesB = generator.ToBytes b
    Assert.Equal(24, bytesA |> Array.length)
    Assert.Equal(1 * 2 + 3 + 4 + 16 * 2, bytesB |> Array.length)

    let valueA = generator.ToValue<Dictionary<int, double>> bytesA
    let valueB = generator.ToValue<Dictionary<string, Guid>> bytesB
    Assert.Equal<Dictionary<int, double>>(a, valueA)
    Assert.Equal<Dictionary<string, Guid>>(b, valueB)
    ()

[<Fact>]
let ``IDictionary`` () =
    let a = new Dictionary<string, Guid>()
    a.Add("head", Guid.NewGuid())
    a.Add("last", Guid.NewGuid())
    let a = a :> IDictionary<string, Guid>
    let bytes = generator.ToBytes a
    Assert.Equal(1 * 2 + 4 * 2 + 16 * 2, bytes |> Array.length)
    let value = generator.ToValue<IDictionary<string, Guid>> bytes
    Assert.Equal<IDictionary<string, Guid>>(a, value)
    Assert.IsType<Dictionary<string, Guid>> value |> ignore
    ()

[<Fact>]
let ``IReadOnlyDictionary`` () =
    let a = new Dictionary<int, decimal>()
    a.Add(-1, decimal 1.1)
    a.Add(-3, decimal 3.3)
    a.Add(Int32.MaxValue, decimal UInt32.MaxValue)
    let a = a :> IReadOnlyDictionary<int, decimal>
    let bytes = generator.ToBytes a
    Assert.Equal(60, bytes |> Array.length)
    let value = generator.ToValue<IReadOnlyDictionary<int, decimal>> bytes
    Assert.Equal<IReadOnlyDictionary<int, decimal>>(a, value)
    Assert.IsType<Dictionary<int, decimal>> value |> ignore
    ()
