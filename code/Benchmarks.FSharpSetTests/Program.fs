namespace Mikodev.Binary.Benchmarks.FSharpSetTests

open BenchmarkDotNet.Configs;
open BenchmarkDotNet.Jobs;
open BenchmarkDotNet.Running;
open BenchmarkDotNet.Toolchains.InProcess.Emit;

module Program =
    [<EntryPoint>]
    let Main _ =
        let config = ManualConfig.Create(DefaultConfig.Instance)
        config.AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance)) |> ignore
        BenchmarkRunner.Run<SetBenchmarks>(config) |> ignore
        0
