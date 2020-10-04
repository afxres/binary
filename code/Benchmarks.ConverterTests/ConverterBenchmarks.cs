﻿using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System;

namespace Mikodev.Binary.Benchmarks.ConverterTests
{
    [MemoryDiagnoser]
    public class ConverterBenchmarks
    {
        private byte[] buffer;

        private int number;

        private byte[] encodeBytes;

        private byte[] encodeAutoBytes;

        private byte[] encodeWithLengthPrefixBytes;

        private Converter<int> converter;

        [Params("constant", "variable", "native")]
        public string Flag;

        [GlobalSetup]
        public void Setup()
        {
            this.buffer = new byte[65536];
            this.number = 31415926;
            this.converter = Flag switch
            {
                "constant" => new ConstantNativeConverter<int>(),
                "variable" => new VariableNativeConverter<int>(),
                "native" => Generator.CreateDefault().GetConverter<int>(),
                _ => throw new NotSupportedException(),
            };
            this.encodeBytes = this.converter.Encode(this.number);
            this.encodeAutoBytes = AllocatorHelper.Invoke(this.number, this.converter.EncodeAuto);
            this.encodeWithLengthPrefixBytes = AllocatorHelper.Invoke(this.number, this.converter.EncodeWithLengthPrefix);
        }

        [Benchmark(Description = "Encode")]
        public byte[] C02()
        {
            return this.converter.Encode(this.number);
        }

        [Benchmark(Description = "Encode (reuse buffer)")]
        public void C01()
        {
            var allocator = new Allocator(this.buffer);
            this.converter.Encode(ref allocator, this.number);
        }

        [Benchmark(Description = "Encode Auto (reuse buffer)")]
        public void C05()
        {
            var allocator = new Allocator(this.buffer);
            this.converter.EncodeAuto(ref allocator, this.number);
        }

        [Benchmark(Description = "Encode With Length Prefix (reuse buffer)")]
        public void C06()
        {
            var allocator = new Allocator(this.buffer);
            this.converter.EncodeWithLengthPrefix(ref allocator, this.number);
        }

        [Benchmark(Description = "Decode (by bytes)")]
        public int C04()
        {
            return converter.Decode(this.encodeBytes);
        }

        [Benchmark(Description = "Decode (by span)")]
        public int C03()
        {
            return converter.Decode(new ReadOnlySpan<byte>(this.encodeBytes));
        }

        [Benchmark(Description = "Decode Auto (by span)")]
        public int C07()
        {
            var span = new ReadOnlySpan<byte>(this.encodeAutoBytes);
            return converter.DecodeAuto(ref span);
        }

        [Benchmark(Description = "Decode With Length Prefix (by span)")]
        public int C08()
        {
            var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
            return converter.DecodeWithLengthPrefix(ref span);
        }
    }
}