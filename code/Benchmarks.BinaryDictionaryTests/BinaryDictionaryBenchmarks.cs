namespace Mikodev.Binary.Benchmarks.BinaryDictionaryTests;

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

public class BinaryDictionaryBenchmarks
{
    private delegate int GetValueOrDefault(ref byte source, int length);

    private byte[][] keys3 = null!;

    private byte[][] keys7 = null!;

    private GetValueOrDefault functorLongData3 = null!;

    private GetValueOrDefault functorHashCode3 = null!;

    private GetValueOrDefault functorLongData7 = null!;

    private GetValueOrDefault functorHashCode7 = null!;

    [GlobalSetup]
    public void Setup()
    {
        var data3 = new[]
        {
            "Id",
            "Data",
            "Message",
        };
        var data7 = new[]
        {
            "123456789ABCDEF",
            "",
            "abcdefghijklmn",
            "+-*/",
            "ABCDEFGHIJKLM",
            "!@#$%^&*",
            "cba987654321",
        };

        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        var createLongData = methods.Single(x => x.Name is "CreateLongDataDictionary");
        var createHashCode = methods.Single(x => x.Name is "CreateHashCodeDictionary").MakeGenericMethod(typeof(int));

        GetValueOrDefault CreateLongData(byte[][] keys)
        {
            var source = keys.Select(x => new ReadOnlyMemory<byte>(x)).ToImmutableArray();
            var target = createLongData.Invoke(null, new[] { (object)source });
            var result = Delegate.CreateDelegate(typeof(GetValueOrDefault), target!, "GetValue");
            return (GetValueOrDefault)result;
        }

        GetValueOrDefault CreateHashCode(byte[][] keys)
        {
            var source = keys.Select((x, i) => KeyValuePair.Create(new ReadOnlyMemory<byte>(x), i)).ToImmutableArray();
            var target = createHashCode.Invoke(null, new[] { (object)source, -1 });
            var result = Delegate.CreateDelegate(typeof(GetValueOrDefault), target!, "GetValue");
            return (GetValueOrDefault)result;
        }

        this.keys3 = data3.Select(Encoding.UTF8.GetBytes).ToArray();
        this.keys7 = data7.Select(Encoding.UTF8.GetBytes).ToArray();

        this.functorLongData3 = CreateLongData(this.keys3);
        this.functorLongData7 = CreateLongData(this.keys7);
        this.functorHashCode3 = CreateHashCode(this.keys3);
        this.functorHashCode7 = CreateHashCode(this.keys7);

        Trace.Assert(this.keys3.Select(x => x.Length).SequenceEqual(new[] { 2, 4, 7 }));
        Trace.Assert(this.keys7.Select(x => x.Length).SequenceEqual(new[] { 15, 0, 14, 4, 13, 8, 12 }));
        Trace.Assert(Invoke(this.functorLongData3, this.keys3) == 3);
        Trace.Assert(Invoke(this.functorHashCode3, this.keys3) == 3);
        Trace.Assert(Invoke(this.functorLongData7, this.keys7) == 21);
        Trace.Assert(Invoke(this.functorHashCode7, this.keys7) == 21);
    }

    private static int Invoke(GetValueOrDefault functor, byte[][] keys)
    {
        var result = 0;
        foreach (var key in keys)
            result += functor.Invoke(ref MemoryMarshal.GetArrayDataReference(key), key.Length);
        return result;
    }

    [Benchmark(Description = "Long Data 3 Keys")]
    public int L3()
    {
        return Invoke(this.functorLongData3, this.keys3);
    }

    [Benchmark(Description = "Hash Code 3 Keys")]
    public int H3()
    {
        return Invoke(this.functorHashCode3, this.keys3);
    }

    [Benchmark(Description = "Long Data 7 Keys")]
    public int L7()
    {
        return Invoke(this.functorLongData7, this.keys7);
    }

    [Benchmark(Description = "Hash Code 7 Keys")]
    public int H7()
    {
        return Invoke(this.functorHashCode7, this.keys7);
    }
}
