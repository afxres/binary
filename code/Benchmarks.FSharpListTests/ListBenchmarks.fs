namespace Mikodev.Binary.Benchmarks.FSharpListTests

open BenchmarkDotNet.Attributes
open Mikodev.Binary
open Mikodev.Binary.Benchmarks.Abstractions
open System
open System.Linq
open System.Runtime.InteropServices

[<MemoryDiagnoser>]
type ListBenchmarks() =
    let mutable buffer : byte array = null

    let mutable intConverter : Converter<int> = null

    let mutable intListConverter : Converter<List<int>> = null

    let mutable intList = List.empty<int>

    let mutable intListBuffer : byte array = null

    let mutable stringConverter : Converter<string> = null

    let mutable stringListConverter : Converter<List<string>> = null

    let mutable stringList = List.empty<string>

    let mutable stringListBuffer : byte array = null

    [<Params(0, 1, 4, 16, 1024)>]
    member val public Count = 0 with get, set

    [<GlobalSetup>]
    member me.Setup() =
        buffer <- Array.zeroCreate 65536
        let generator =
            Generator.CreateDefaultBuilder()
                .AddConverter(BinaryStringConverter())
                .AddFSharpConverterCreators()
                .Build()
        intConverter <- generator.GetConverter<_>()
        intListConverter <- generator.GetConverter<_>()
        intList <- Enumerable.Range(0, me.Count) |> Seq.toList
        intListBuffer <- intListConverter.Encode intList
        stringConverter <- generator.GetConverter<_>()
        stringListConverter <- generator.GetConverter<_>()
        stringList <- intList |> List.map string
        stringListBuffer <- stringListConverter.Encode stringList
        ()

    [<Benchmark(Description = "Decode List Of Int (converter)")>]
    member __.LD01() : List<int> =
        intListConverter.Decode intListBuffer

    [<Benchmark(Description = "Decode List Of Int (cast span)")>]
    member __.LD02() : List<int> =
        let data = MemoryMarshal.Cast<byte, int>(ReadOnlySpan intListBuffer)
        let mutable list = []
        for i = data.Length - 1 downto 0 do
            list <- data.[i] :: list
        list

    [<Benchmark(Description = "Decode List Of Int (for loop)")>]
    member __.LD03() : List<int> =
        let converter = intConverter
        let itemLength = converter.Length
        let span = ReadOnlySpan intListBuffer
        let size, _ = Math.DivRem(span.Length, itemLength)
        let mutable list = []
        for i = size - 1 downto 0 do
            let data = span.Slice(i * itemLength, itemLength)
            list <- converter.Decode &data :: list
        list

    [<Benchmark(Description = "Decode List Of String (converter)")>]
    member __.LD11() : List<string> =
        stringListConverter.Decode stringListBuffer

    [<Benchmark(Description = "Decode List Of String (resize array)")>]
    member __.LD12() : List<string> =
        let converter = stringConverter
        let mutable span = ReadOnlySpan stringListBuffer
        let data = ResizeArray<_>()
        while not span.IsEmpty do
            data.Add(converter.DecodeAuto &span)
        let mutable list = []
        for i = data.Count - 1 downto 0 do
            list <- data.[i] :: list
        list

    static member private Decode(converter : Converter<_>, span : byref<ReadOnlySpan<byte>>) : List<_> =
        if not span.IsEmpty then
            let head = converter.DecodeAuto &span
            let tail = ListBenchmarks.Decode(converter, &span)
            head :: tail
        else
            []

    [<Benchmark(Description = "Decode List Of String (recursively)")>]
    member __.LD14() : List<string> =
        let converter = stringConverter
        let mutable span = ReadOnlySpan stringListBuffer
        ListBenchmarks.Decode(converter, &span)
