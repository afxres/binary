﻿namespace Mikodev.Binary.Benchmarks.AllocatorTests;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class AllocatorBenchmarks
{
    private byte[]? buffer0;

    private byte[]? buffer1;

    private byte[]? buffer1024;

    private int maxCapacity512;

    private int maxCapacity2048;

    private AllocatorAction<byte[]?> ignoreAction = null!;

    private AllocatorAction<byte[]?> appendAction = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer0 = [];
        this.buffer1 = [0x7F];
        this.buffer1024 = new byte[1024];
        this.maxCapacity512 = 512;
        this.maxCapacity2048 = 2048;
        this.ignoreAction = (ref Allocator allocator, byte[]? data) => { };
        this.appendAction = (ref Allocator allocator, byte[]? data) => Allocator.Append(ref allocator, data);
    }

    [Benchmark(Description = "Append (buffer length 1024, length: 0)")]
    public void A01()
    {
        var allocator = new Allocator(this.buffer1024);
        Allocator.Append(ref allocator, this.buffer0);
    }

    [Benchmark(Description = "Append (buffer length 1024, length: 1)")]
    public void A02()
    {
        var allocator = new Allocator(this.buffer1024);
        Allocator.Append(ref allocator, this.buffer1);
    }

    [Benchmark(Description = "Append (buffer length 1024 with max capacity 512, length: 0)")]
    public void A03()
    {
        var allocator = new Allocator(this.buffer1024, this.maxCapacity512);
        Allocator.Append(ref allocator, this.buffer0);
    }

    [Benchmark(Description = "Append (buffer length 1024 with max capacity 512, length: 1)")]
    public void A04()
    {
        var allocator = new Allocator(this.buffer1024, this.maxCapacity512);
        Allocator.Append(ref allocator, this.buffer1);
    }

    [Benchmark(Description = "Append (buffer length 1024 with max capacity 2048, length: 0)")]
    public void A05()
    {
        var allocator = new Allocator(this.buffer1024, this.maxCapacity2048);
        Allocator.Append(ref allocator, this.buffer0);
    }

    [Benchmark(Description = "Append (buffer length 1024 with max capacity 2048, length: 1)")]
    public void A06()
    {
        var allocator = new Allocator(this.buffer1024, this.maxCapacity2048);
        Allocator.Append(ref allocator, this.buffer1);
    }

    [Benchmark(Description = "Invoke (do nothing)")]
    public byte[] I00()
    {
        return Allocator.Invoke(null, this.ignoreAction);
    }

    [Benchmark(Description = "Invoke (append length: 0)")]
    public byte[] I01()
    {
        return Allocator.Invoke(this.buffer0, this.appendAction);
    }

    [Benchmark(Description = "Invoke (append length: 1)")]
    public byte[] I02()
    {
        return Allocator.Invoke(this.buffer1, this.appendAction);
    }
}
