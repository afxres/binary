namespace Mikodev.Binary.Tests.External;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

public class HashCodeDictionaryTests
{
    private delegate int HashCode(ref byte source, int length);

    private delegate object CreateDictionary<T>(ImmutableArray<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T? @default);

    private delegate T GetValue<T>(ref byte source, int length);

    private static T GetInternalDelegate<T>(string name) where T : Delegate
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryModule");
        Assert.NotNull(type);
        var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (T)Delegate.CreateDelegate(typeof(T), Assert.IsAssignableFrom<MethodInfo>(method));
    }

    private static HashCode GetHashCodeDelegate()
    {
        return GetInternalDelegate<HashCode>("GetHashCode");
    }

    private static CreateDictionary<T> GetCreateDictionaryDelegate<T>()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
        Assert.NotNull(type);
        var method = type.GetMethod("CreateHashCodeDictionary", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (CreateDictionary<T>)Delegate.CreateDelegate(typeof(CreateDictionary<T>), Assert.IsAssignableFrom<MethodInfo>(method).MakeGenericMethod(typeof(T)));
    }

    private static GetValue<T> GetGetValueDelegate<T>(object dictionary)
    {
        var method = dictionary.GetType().GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);
        return (GetValue<T>)Delegate.CreateDelegate(typeof(GetValue<T>), dictionary, Assert.IsAssignableFrom<MethodInfo>(method));
    }

    [Theory(DisplayName = "Hash Conflicts")]
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

    [Theory(DisplayName = "Duplicate Keys")]
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

    [Fact(DisplayName = "Integration Test With System Member Names")]
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

    [Theory(DisplayName = "Key Not Found")]
    [InlineData(new[] { 12345678 }, new[] { 87654321 })]
    [InlineData(new[] { 333, 55555 }, new[] { 22, 4444 })]
    [InlineData(new[] { 9, 1234567890 }, new[] { 3, 432, 67 })]
    public void DictionaryQueryNotFound(int[] values, int[] others)
    {
        var create = GetCreateDictionaryDelegate<string>();
        var arguments = values.Select(x => KeyValuePair.Create(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString())), x.ToString())).ToImmutableArray();
        var result = create.Invoke(arguments, null);
        var query = GetGetValueDelegate<string>(result);
        Assert.NotNull(result);
        for (var i = 0; i < others.Length; i++)
        {
            var buffer = Encoding.UTF8.GetBytes(others[i].ToString()).AsSpan();
            var actual = query.Invoke(ref MemoryMarshal.GetReference(buffer), buffer.Length);
            Assert.Null(actual);
        }
    }
}
