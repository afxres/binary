using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Mikodev.Binary.Tests.External
{
    public class HashCodeDictionaryTests
    {
        private delegate int HashCode(ref byte source, int length);

        private delegate object CreateDictionary<T>(ImmutableArray<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T @default);

        private delegate T GetValue<T>(ref byte source, int length);

        private static T GetInternalDelegate<T>(string name) where T : Delegate
        {
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
            Assert.NotNull(type);
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        private static HashCode GetHashCodeDelegate()
        {
            return GetInternalDelegate<HashCode>("GetHashCode");
        }

        private static CreateDictionary<T> GetCreateDictionaryDelegate<T>()
        {
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
            Assert.NotNull(type);
            var method = type.GetMethod("CreateHashCodeDictionary", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(T));
            Assert.NotNull(method);
            return (CreateDictionary<T>)Delegate.CreateDelegate(typeof(CreateDictionary<T>), method);
        }

        private static GetValue<T> GetGetValueDelegate<T>(object dictionary, string name)
        {
            var method = dictionary.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            return (GetValue<T>)Delegate.CreateDelegate(typeof(GetValue<T>), dictionary, method);
        }

        private static GetValue<T> GetGetValueDelegate<T>(object dictionary)
        {
            return GetGetValueDelegate<T>(dictionary, "GetValue");
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

            var arguments = buffers.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(x), x.Length)).ToImmutableArray();
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
            var arguments = values.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString())), x)).ToImmutableArray();
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

            var arguments = names.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x)), x)).ToImmutableArray();
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
