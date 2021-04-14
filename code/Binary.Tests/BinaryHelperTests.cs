using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class BinaryHelperTests
    {
        private delegate bool Equality(ref byte source, int length, byte[] buffer);

        private delegate int HashCode(ref byte source, int length);

        private delegate int Capacity(int capacity);

        private delegate object CreateDictionary<T>(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T @default);

        private delegate T GetValue<T>(ref byte source, int length);

        private T GetInternalDelegate<T>(string name) where T : Delegate
        {
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryHelper");
            Assert.NotNull(type);
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        private Equality GetEqualityDelegate()
        {
            return GetInternalDelegate<Equality>("GetEquality");
        }

        private HashCode GetHashCodeDelegate()
        {
            return GetInternalDelegate<HashCode>("GetHashCode");
        }

        private Capacity GetCapacityDelegate()
        {
            return GetInternalDelegate<Capacity>("GetCapacity");
        }

        private CreateDictionary<T> GetCreateDictionaryDelegate<T>()
        {
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryDictionary`1");
            Assert.NotNull(type);
            var method = type.MakeGenericType(typeof(T)).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (CreateDictionary<T>)Delegate.CreateDelegate(typeof(CreateDictionary<T>), method);
        }

        private GetValue<T> GetGetValueDelegate<T>(object dictionary, string name)
        {
            var method = dictionary.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (GetValue<T>)Delegate.CreateDelegate(typeof(GetValue<T>), dictionary, method);
        }

        private GetValue<T> GetGetValueDelegate<T>(object dictionary)
        {
            return GetGetValueDelegate<T>(dictionary, "GetValue");
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
            var result = equality.Invoke(ref Unsafe.NullRef<byte>(), 0, Array.Empty<byte>());
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
            var result = new List<int>(names.Length);
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
            var result = function.Invoke(ref Unsafe.NullRef<byte>(), 0);
            Assert.Equal(0, result);
        }

        [Fact(DisplayName = "Prime Table")]
        public void PrimeTable()
        {
            var systemType = typeof(Dictionary<,>).Assembly.GetTypes().Single(x => x.Namespace is "System.Collections" && x.Name is "HashHelpers");
            Assert.NotNull(systemType);
            var systemField = systemType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Single(x => x.Name.Contains("primes"));
            Assert.NotNull(systemField);
            var systemPrimeTable = (IReadOnlyList<int>)systemField.GetValue(null);

            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryHelper");
            var field = type.GetField("primes", BindingFlags.Static | BindingFlags.NonPublic);
            var customPrimeTable = (IReadOnlyList<int>)field.GetValue(null);

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
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryHelper");
            var field = type.GetField("primes", BindingFlags.Static | BindingFlags.NonPublic);
            var customPrimeTable = (IReadOnlyList<int>)field.GetValue(null);

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
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryHelper");
            var field = type.GetField("primes", BindingFlags.Static | BindingFlags.NonPublic);
            var customPrimeTable = (IReadOnlyList<int>)field.GetValue(null);

            var function = GetCapacityDelegate();
            foreach (var i in customPrimeTable)
            {
                var result = function.Invoke(i);
                Assert.Equal(i, result);
            }
        }

        [Theory(DisplayName = "Dictionary (hash conflict)")]
        [InlineData((byte)1, new[] { 49631, 52013 })]
        [InlineData((byte)'A', new[] { 126008, 142545 })]
        [InlineData((byte)'~', new[] { 93527, 154641 })]
        public void DictionaryHashConflict(byte value, int[] sizes)
        {
            var hash = GetHashCodeDelegate();
            var create = GetCreateDictionaryDelegate<int>();
            var buffers = new List<byte[]>();
            foreach (var i in sizes)
            {
                var buffer = new byte[i];
                Array.Fill(buffer, value);
                buffers.Add(buffer);
            }

            var codes = new List<int>();
            foreach (var i in buffers)
            {
                var length = i.Length;
                ref var source = ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(i));
                var result = hash.Invoke(ref source, length);
                codes.Add(result);
            }

            Assert.Equal(sizes.Length, codes.Count);
            _ = Assert.Single(codes.Distinct());

            var arguments = buffers.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(x), x.Length)).ToList();
            var dictionary = create.Invoke(arguments, -1);
            Assert.NotNull(dictionary);
            var query = GetGetValueDelegate<int>(dictionary);

            var actual = new List<int>();
            foreach (var i in buffers)
            {
                var length = i.Length;
                ref var source = ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(i));
                var result = query.Invoke(ref source, length);
                actual.Add(result);
                Assert.Equal(length, result);
            }
            Assert.Equal(sizes, actual);
        }

        [Theory(DisplayName = "Dictionary (duplicate key)")]
        [InlineData(new[] { 1, 33, 1024, 33 })]
        [InlineData(new[] { 2, 2, 3, 4 })]
        [InlineData(new[] { 32768, 65535, 65536, 65536 })]
        public void DictionaryDuplicateKey(int[] values)
        {
            var create = GetCreateDictionaryDelegate<int>();
            var arguments = values.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString())), x)).ToArray();
            var result = create.Invoke(arguments, -1);
            Assert.Null(result);
        }

        [Fact(DisplayName = "Dictionary (system type and member names)")]
        public void DictionarySystemTypeNames()
        {
            var types = typeof(object).Assembly.GetTypes();
            var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();
            var names = types.Select(x => x.Name).Concat(members.Select(x => x.Name)).ToHashSet().ToArray();
            Assert.True(names.Length > 1000);

            var arguments = names.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x)), x)).ToArray();
            var create = GetCreateDictionaryDelegate<string>();
            var dictionary = create.Invoke(arguments, null);
            Assert.NotNull(dictionary);
            var query = GetGetValueDelegate<string>(dictionary);

            var actual = new List<string>();
            foreach (var i in arguments)
            {
                var buffer = i.Key.Span;
                var length = buffer.Length;
                ref var source = ref MemoryMarshal.GetReference(buffer);
                var result = query.Invoke(ref source, length);
                actual.Add(result);
                Assert.Equal(i.Value, result);
            }
            Assert.Equal(names, actual);
        }
    }
}
