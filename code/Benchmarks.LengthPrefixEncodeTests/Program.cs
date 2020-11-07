﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests
{
    internal class Program
    {
        private static void Main()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance);
            _ = config.AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance));
            _ = BenchmarkRunner.Run<LengthPrefixEncodeBenchmarks>(config);
        }
    }
}
