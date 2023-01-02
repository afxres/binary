namespace Mikodev.Binary.Tests.External;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

public class BinaryModuleTests
{
    private struct LongDataSlot
    {
        public ulong Head;

        public ulong Tail;
    }

    private delegate int Capacity(int capacity);

    private delegate uint HashCode(ref byte source, int length);

    private delegate bool Equality(ref byte source, int length, byte[] buffer);

    private static MethodInfo GetInternalMethod(string name)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
        Assert.NotNull(type);
        var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return method;
    }

    private static T GetInternalDelegate<T>(string name) where T : Delegate
    {
        var method = GetInternalMethod(name);
        Assert.NotNull(method);
        return (T)Delegate.CreateDelegate(typeof(T), Assert.IsAssignableFrom<MethodInfo>(method));
    }

    private static HashCode GetHashCodeDelegate()
    {
        return GetInternalDelegate<HashCode>("GetHashCode");
    }

    private static Equality GetEqualityDelegate()
    {
        return GetInternalDelegate<Equality>("GetEquality");
    }

    private static Capacity GetCapacityDelegate()
    {
        return GetInternalDelegate<Capacity>("GetCapacity");
    }

    private static unsafe delegate*<ref byte, int, LongDataSlot> GetLongDataDelegate()
    {
        var method = GetInternalMethod("GetLongData");
        var handle = method.MethodHandle;
        return (delegate*<ref byte, int, LongDataSlot>)handle.GetFunctionPointer();
    }

    [Theory(DisplayName = "Equality (length mismatch)")]
    [InlineData(0, 1)]
    [InlineData(33, 0)]
    [InlineData(47, 21)]
    public void EqualityLengthMismatch(int length, int limits)
    {
        var equality = GetEqualityDelegate();
        var alpha = new byte[length];
        var bravo = new byte[limits];
        var result = equality.Invoke(ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(bravo)), bravo.Length, alpha);
        Assert.NotEqual(length, limits);
        Assert.False(result);
    }

    [Fact(DisplayName = "Equality (length both zero)")]
    public void EqualityLengthBothZero()
    {
        var equality = GetEqualityDelegate();
        var result = equality.Invoke(ref MemoryMarshal.GetReference<byte>(default), 0, Array.Empty<byte>());
        Assert.True(result);
    }

    [Fact(DisplayName = "Equality (random length 0 to 64, equal)")]
    public void EqualityRandomEqual()
    {
        var random = new Random();
        var equality = GetEqualityDelegate();
        for (var i = 0; i < 64; i++)
        {
            var alpha = new byte[i];
            random.NextBytes(alpha);
            var bravo = alpha.ToArray();
            Assert.False(ReferenceEquals(alpha, bravo));
            var result = equality.Invoke(ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(bravo)), bravo.Length, alpha);
            Assert.True(result);
            Assert.Equal(i, alpha.Length);
            Assert.Equal(i, bravo.Length);
            Assert.Equal(alpha, bravo);
        }
    }

    [Fact(DisplayName = "Equality (random length 0 to 32, not equal)")]
    public void EqualityRandomNotEqual()
    {
        var random = new Random();
        var equality = GetEqualityDelegate();
        for (var i = 0; i < 32; i++)
        {
            var alpha = new byte[i];
            random.NextBytes(alpha);
            for (var k = 0; k < i; k++)
            {
                var bravo = alpha.ToArray();
                bravo[k] = (byte)(bravo[k] + i);
                Assert.False(ReferenceEquals(alpha, bravo));
                var result = equality.Invoke(ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(bravo)), bravo.Length, alpha);
                Assert.False(result);
                Assert.Equal(i, alpha.Length);
                Assert.Equal(i, bravo.Length);
                Assert.NotEqual(alpha, bravo);
            }
        }
    }

    [Fact(DisplayName = "Hash Code (system type names)")]
    public void HashCodeTest()
    {
        var names = typeof(object).Assembly.GetTypes().Select(x => x.Name).Distinct().ToArray();
        var function = GetHashCodeDelegate();
        var result = new List<uint>(names.Length);
        foreach (var i in names)
        {
            var buffer = Encoding.UTF8.GetBytes(i);
            var length = buffer.Length;
            ref var source = ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(buffer));
            var target = function.Invoke(ref source, length);
            result.Add(target);
        }
        var set = result.ToHashSet();
        var percent = 1.0 * set.Count / result.Count;

        // 哈希冲突不应当高于百分之一
        Assert.True(percent >= 0.99);
        Assert.True(percent <= 1.00);
        Assert.True(names.Length > 1000);
        Assert.Equal(names.Length, result.Count);
    }

    [Fact(DisplayName = "Hash Code (empty buffer)")]
    public void HashCodeEmptyBuffer()
    {
        var function = GetHashCodeDelegate();
        var result = function.Invoke(ref MemoryMarshal.GetReference<byte>(default), 0);
        Assert.Equal(0U, result);
    }

    [Fact(DisplayName = "Prime Table")]
    public void PrimeTable()
    {
        var systemType = typeof(Dictionary<,>).Assembly.GetTypes().Single(x => x.Namespace is "System.Collections" && x.Name is "HashHelpers");
        Assert.NotNull(systemType);
        var systemField = systemType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Single(x => x.Name.Contains("primes"));
        Assert.NotNull(systemField);
        var systemPrimeTable = Assert.IsAssignableFrom<IReadOnlyList<int>>(systemField.GetValue(null));

        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
        var field = Assert.IsAssignableFrom<FieldInfo>(type.GetField("Primes", BindingFlags.Static | BindingFlags.NonPublic));
        var customPrimeTable = Assert.IsAssignableFrom<IReadOnlyList<int>>(field.GetValue(null));

        Assert.NotNull(systemPrimeTable);
        Assert.NotNull(customPrimeTable);
        Assert.Equal(systemPrimeTable, customPrimeTable);
        Assert.False(ReferenceEquals(systemPrimeTable, customPrimeTable));
        Assert.True(MemoryExtensions.SequenceEqual(new ReadOnlySpan<int>(systemPrimeTable.ToArray()), new ReadOnlySpan<int>(customPrimeTable.ToArray())));
    }

    [Theory(DisplayName = "Capacity")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(63356)]
    public void CapacityTest(int capacity)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
        var field = Assert.IsAssignableFrom<FieldInfo>(type.GetField("Primes", BindingFlags.Static | BindingFlags.NonPublic));
        var customPrimeTable = Assert.IsAssignableFrom<IReadOnlyList<int>>(field.GetValue(null));

        var function = GetCapacityDelegate();
        var result = function.Invoke(capacity);
        Assert.True(result >= capacity);
        Assert.Contains(result, customPrimeTable);
    }

    [Fact(DisplayName = "Capacity (overflow)")]
    public void CapacityOverflow()
    {
        var function = GetCapacityDelegate();
        var error = Assert.Throws<ArgumentException>(() => function.Invoke(int.MaxValue));
        var message = "Maximum capacity has been reached.";
        Assert.Null(error.ParamName);
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Capacity (match prime table)")]
    public void CapacityEqual()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
        var field = Assert.IsAssignableFrom<FieldInfo>(type.GetField("Primes", BindingFlags.Static | BindingFlags.NonPublic));
        var customPrimeTable = Assert.IsAssignableFrom<IReadOnlyList<int>>(field.GetValue(null));

        var function = GetCapacityDelegate();
        foreach (var i in customPrimeTable)
        {
            var result = function.Invoke(i);
            Assert.Equal(i, result);
        }
    }

    [Fact(DisplayName = "Long Data (zero length)")]
    public unsafe void LongDataZeroLength()
    {
        var function = GetLongDataDelegate();
        var result = function(ref MemoryMarshal.GetReference<byte>(default), 0);
        Assert.Equal(0UL, result.Head);
        Assert.Equal(0UL, result.Tail);
    }

    [Fact(DisplayName = "Long Data (all possible sizes)")]
    public unsafe void LongDataAllPossibleSizes()
    {
        var function = GetLongDataDelegate();
        var origin = "123456789ABCDEF";
        var ignore = new List<string>();
        for (var length = 0; length <= 15; length++)
        {
            var source = origin.Substring(0, length);
            var buffer = Encoding.UTF8.GetBytes(source.ToString());
            var result = function(ref MemoryMarshal.GetArrayDataReference(buffer), buffer.Length);
            Assert.Equal((uint)length, (uint)(result.Tail & 0xFFUL));

            var headSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref result.Head), sizeof(long));
            var tailSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref result.Tail), sizeof(long));
            var headPart = (length & 8) is 0 ? Array.Empty<byte>() : headSpan.ToArray();
            var tailPart = tailSpan.Slice(1, length & 7).ToArray();
            var expect = Enumerable.Concat(tailPart, headPart).ToArray();
            var actual = Encoding.UTF8.GetString(expect);
            Assert.Equal(source, actual);
            var expected = source.ToCharArray().ToHashSet();
            var response = actual.ToCharArray().ToHashSet();
            Assert.Equal(expected, response);
            ignore.Add(actual);
        }
        Assert.Equal(16, ignore.Count);
        Assert.Equal(string.Empty, ignore.First());
        Assert.Equal(origin, ignore.Last());
    }

    [Fact(DisplayName = "Long Data (random bytes)")]
    public unsafe void LongDataRandomBytes()
    {
        var function = GetLongDataDelegate();
        var random = new Random();
        var ignore = new List<byte[]>();
        for (var length = 1; length <= 15; length++)
        {
            for (var k = 0; k < 1024; k++)
            {
                var buffer = new byte[length];
                random.NextBytes(buffer);
                var result = function(ref MemoryMarshal.GetArrayDataReference(buffer), buffer.Length);
                Assert.Equal((uint)length, (uint)(result.Tail & 0xFFUL));

                var headSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref result.Head), sizeof(long));
                var tailSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref result.Tail), sizeof(long));
                var headPart = (length & 8) is 0 ? Array.Empty<byte>() : headSpan.ToArray();
                var tailPart = tailSpan.Slice(1, length & 7).ToArray();
                var expect = Enumerable.Concat(tailPart, headPart).ToArray();
                Assert.Equal(buffer, expect);

                var remain = tailSpan.Slice(1 + (length & 7));
                Assert.True(remain.IsEmpty || remain.ToArray().All(x => x is 0));
                ignore.Add(buffer);
            }
        }
        Assert.Equal(15360, ignore.Count);
    }
}
