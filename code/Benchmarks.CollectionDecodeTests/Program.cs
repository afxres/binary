﻿namespace Mikodev.Binary.Benchmarks.CollectionDecodeTests;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

internal class Program
{
    private static void Main()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance);
        _ = config.AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance)).WithOptions(ConfigOptions.JoinSummary);
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll(config);
    }
}
