using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Mikodev.Binary.Benchmarks
{
    internal class Program
    {
        private static void Main()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(Job.MediumRun.With(InProcessEmitToolchain.Instance));
            _ = BenchmarkRunner.Run<StandardBenchmark>(config);
        }
    }
}
