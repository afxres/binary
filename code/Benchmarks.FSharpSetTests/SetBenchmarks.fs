namespace Mikodev.Binary.Benchmarks.FSharpSetTests

open BenchmarkDotNet.Attributes
open Mikodev.Binary
open Mikodev.Binary.Benchmarks.Abstractions
open System
open System.Linq

[<MemoryDiagnoser>]
type SetBenchmarks() =
    let mutable buffer: byte array = null

    let mutable intSetConverter: Converter<Set<int>> = null

    let mutable intSet = Set.empty<int>

    let mutable intSetBuffer: byte array = null

    [<Params(0, 1, 1024)>]
    member val public Count = 0 with get, set

    [<GlobalSetup>]
    member me.Setup() =
        buffer <- Array.zeroCreate 65536
        let generator = Generator.CreateDefaultBuilder().AddConverter(BinaryStringConverter()).AddFSharpConverterCreators().Build()
        intSetConverter <- generator.GetConverter<_>()
        intSet <- Enumerable.Range(0, me.Count) |> Set
        intSetBuffer <- intSetConverter.Encode intSet
        ()

    [<Benchmark(Description = "Encode Set Of Int (converter)")>]
    member __.SE01() =
        let mutable allocator = Allocator(Span buffer)
        intSetConverter.Encode(&allocator, intSet)
        ()

    [<Benchmark(Description = "Decode Set Of Int (converter)")>]
    member __.SD01() : Set<int> = intSetConverter.Decode intSetBuffer
