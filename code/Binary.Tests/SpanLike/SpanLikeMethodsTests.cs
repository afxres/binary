﻿namespace Mikodev.Binary.Tests.SpanLike;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

public class SpanLikeMethodsTests
{
    private const int MaxLevels = 32;

    private const int NewLength = 64;

    private delegate List<E> GetListRecursively<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int cursor);

    private delegate E[] GetArrayRecursively<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int cursor);

    private delegate List<E> GetList<E>(Converter<E> converter, ReadOnlySpan<byte> span);

    private delegate E[] GetArray<E>(Converter<E> converter, ReadOnlySpan<byte> span, out int actual);

    private static D GetMethod<D>() where D : Delegate
    {
        var arguments = typeof(D).GetGenericArguments();
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "SpanLikeMethods");
        var method = type.GetMethod(typeof(D).Name.Split('`').First(), BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var result = (D)Delegate.CreateDelegate(typeof(D), method.MakeGenericMethod(arguments));
        return result;
    }

    public static IEnumerable<object[]> CollectionData()
    {
        yield return new object[] { ImmutableArray.Create<int>(), 0, 0 };
        yield return new object[] { ImmutableArray.Create<string>(), 0, 0 };
        yield return new object[] { Enumerable.Range(0, 1).ToImmutableArray(), 1, 1 };
        yield return new object[] { Enumerable.Range(0, 1).Select(x => x.ToString()).ToImmutableArray(), 1, 1 };
        yield return new object[] { Enumerable.Range(0, MaxLevels - 1).ToImmutableArray(), MaxLevels - 1, MaxLevels - 1 };
        yield return new object[] { Enumerable.Range(0, MaxLevels - 1).Select(x => x.ToString()).ToImmutableArray(), MaxLevels - 1, MaxLevels - 1 };
        yield return new object[] { Enumerable.Range(0, MaxLevels).ToImmutableArray(), MaxLevels, MaxLevels };
        yield return new object[] { Enumerable.Range(0, MaxLevels).Select(x => x.ToString()).ToImmutableArray(), MaxLevels, MaxLevels };
        yield return new object[] { Enumerable.Range(0, MaxLevels + 1).ToImmutableArray(), MaxLevels, NewLength };
        yield return new object[] { Enumerable.Range(0, MaxLevels + 1).Select(x => x.ToString()).ToImmutableArray(), MaxLevels, NewLength };
        yield return new object[] { Enumerable.Range(0, MaxLevels + 127).ToImmutableArray(), MaxLevels, NewLength };
        yield return new object[] { Enumerable.Range(0, MaxLevels + 127).Select(x => x.ToString()).ToImmutableArray(), MaxLevels, NewLength };
    }

    [Theory(DisplayName = "Get List Test")]
    [MemberData(nameof(CollectionData))]
    public void GetListTest<E>(IReadOnlyCollection<E> source, int countExpected, int capacityExpected)
    {
        var getList = GetMethod<GetList<E>>();
        var getListRecursively = GetMethod<GetListRecursively<E>>();

        var generator = Generator.CreateDefault();
        var buffer = generator.Encode(source);
        var converter = generator.GetConverter<E>();
        var result = getList.Invoke(converter, buffer);
        var span = new ReadOnlySpan<byte>(buffer);
        var resultRecursively = getListRecursively.Invoke(converter, ref span, 0);

        Assert.Equal(source.Count, result.Count);
        Assert.Equal(source, result);
        Assert.Equal(countExpected, resultRecursively.Count);
        Assert.Equal(source.Take(countExpected), resultRecursively);
        Assert.Equal(capacityExpected, resultRecursively.Capacity);
    }

    [Theory(DisplayName = "Get Array Test")]
    [MemberData(nameof(CollectionData))]
    public void GetArrayTest<E>(IReadOnlyCollection<E> source, int countExpected, int capacityExpected)
    {
        var getArray = GetMethod<GetArray<E>>();
        var getArrayRecursively = GetMethod<GetArrayRecursively<E>>();

        var generator = Generator.CreateDefault();
        var buffer = generator.Encode(source);
        var converter = generator.GetConverter<E>();
        var result = getArray.Invoke(converter, buffer, out var resultActual);
        var span = new ReadOnlySpan<byte>(buffer);
        var resultRecursively = getArrayRecursively.Invoke(converter, ref span, 0);

        Assert.Equal(source.Count, resultActual);
        Assert.Equal(source, result.Take(resultActual));
        Assert.Equal(countExpected, Math.Min(resultRecursively.Length, MaxLevels));
        Assert.Equal(source.Take(countExpected), resultRecursively.Take(countExpected));
        Assert.Equal(capacityExpected, resultRecursively.Length);
    }
}
