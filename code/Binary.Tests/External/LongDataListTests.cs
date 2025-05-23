﻿namespace Mikodev.Binary.Tests.External;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

public class LongDataListTests
{
    private delegate object CreateDictionary(ImmutableArray<ImmutableArray<byte>> items, out int error);

    private delegate int GetValue(ref byte source, int length);

    private static CreateDictionary GetCreateDictionaryDelegate()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
        Assert.NotNull(type);
        var method = type.GetMethod("CreateLongDataList", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (CreateDictionary)Delegate.CreateDelegate(typeof(CreateDictionary), method);
    }

    private static GetValue GetGetValueDelegate(object dictionary)
    {
        var method = dictionary.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);
        return (GetValue)Delegate.CreateDelegate(typeof(GetValue), dictionary, method);
    }

    [Theory(DisplayName = "Duplicate Keys")]
    [InlineData(new[] { 1, 33, 1024, 33 }, 3)]
    [InlineData(new[] { 2, 2, 3, 4 }, 1)]
    [InlineData(new[] { 32768, 65535, 65536, 65536 }, 3)]
    public void DictionaryDuplicateKey(int[] values, int index)
    {
        var create = GetCreateDictionaryDelegate();
        var arguments = values.Select(x => Encoding.UTF8.GetBytes(x.ToString()).ToImmutableArray()).ToImmutableArray();
        var result = create.Invoke(arguments, out var error);
        Assert.Null(result);
        Assert.Equal(index, error);
    }

    [Theory(DisplayName = "Zero Character Key")]
    [InlineData(new[] { 0, 1, 2 })]
    [InlineData(new[] { 0, 2, 4, 6, 8 })]
    [InlineData(new[] { 2, 3, 4, 5, 6, 7, 8 })]
    public void DictionaryZeroCharacterKey(int[] sizes)
    {
        var create = GetCreateDictionaryDelegate();
        var values = sizes.Select(x => new string('\0', x)).ToList();
        var arguments = values.Select(x => Encoding.UTF8.GetBytes(x.ToString()).ToImmutableArray()).ToImmutableArray();
        var result = create.Invoke(arguments, out var error);
        var query = GetGetValueDelegate(result);
        Assert.NotNull(result);
        Assert.Equal(-1, error);
        for (var i = 0; i < arguments.Length; i++)
        {
            var buffer = arguments[i].AsSpan();
            var actual = query.Invoke(ref MemoryMarshal.GetReference(buffer), buffer.Length);
            Assert.Equal(i, actual);
        }
    }

    [Theory(DisplayName = "Long Key Length Invalid")]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(16)]
    [InlineData(int.MaxValue)]
    public void DictionaryQueryLengthOverflow(int length)
    {
        var create = GetCreateDictionaryDelegate();
        var values = new[] { string.Empty };
        var arguments = values.Select(x => Encoding.UTF8.GetBytes(x.ToString()).ToImmutableArray()).ToImmutableArray();
        var result = create.Invoke(arguments, out var error);
        var query = GetGetValueDelegate(result);
        Assert.NotNull(result);
        Assert.Equal(-1, error);
        var actual = query.Invoke(ref Unsafe.NullRef<byte>(), length);
        Assert.Equal(-1, actual);
    }

    [Theory(DisplayName = "Key Not Found")]
    [InlineData(new[] { 1, 333, 88888888 }, new[] { 3, 432, 98765432 })]
    [InlineData(new[] { 8, 666, 44444, 11111111 }, new[] { 128, 4096 })]
    [InlineData(new[] { 12345678 }, new[] { 87654321 })]
    public void DictionaryQueryNotFound(int[] values, int[] others)
    {
        var create = GetCreateDictionaryDelegate();
        var arguments = values.Select(x => Encoding.UTF8.GetBytes(x.ToString()).ToImmutableArray()).ToImmutableArray();
        var result = create.Invoke(arguments, out var error);
        var query = GetGetValueDelegate(result);
        Assert.NotNull(result);
        Assert.Equal(-1, error);
        for (var i = 0; i < others.Length; i++)
        {
            var buffer = Encoding.UTF8.GetBytes(others[i].ToString()).AsSpan();
            var actual = query.Invoke(ref MemoryMarshal.GetReference(buffer), buffer.Length);
            Assert.Equal(-1, actual);
        }
    }
}
