namespace Mikodev.Binary.Tests.Sequence;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FrozenCollectionTests
{
    private static Action<T> MakeDelegate<T>(Type interfaceDefinition, Delegate @delegate)
    {
        var definition = @delegate.Method.GetGenericMethodDefinition();
        var genericArguments = typeof(T).GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceDefinition)
            .GetGenericArguments();
        var method = definition.MakeGenericMethod(genericArguments);
        var invoke = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), method);
        return invoke;
    }

    private static void FrozenDictionaryForceBaseTypeConverterInternal<K, V>(FrozenDictionary<K, V> source) where K : notnull
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<FrozenDictionary<K, V>>();
        var converterGenericArgument = Converter.GetGenericArgument(converter);
        Assert.Equal(typeof(FrozenDictionary<K, V>), converterGenericArgument);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
    }

    private static void FrozenSetForceBaseTypeConverterInternal<T>(FrozenSet<T> source)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<FrozenSet<T>>();
        var converterGenericArgument = Converter.GetGenericArgument(converter);
        Assert.Equal(typeof(FrozenSet<T>), converterGenericArgument);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
    }

    public static IEnumerable<object[]> FrozenDictionaryData()
    {
        var source = Enumerable.Range(0, 100).ToList();
        var a = source.Select(x => KeyValuePair.Create(x, x)).ToFrozenDictionary(optimizeForReading: true);
        var b = source.Select(x => KeyValuePair.Create(x, x.ToString())).ToFrozenDictionary(optimizeForReading: true);
        var c = source.Select(x => KeyValuePair.Create(x.ToString(), x)).ToFrozenDictionary(optimizeForReading: true);
        var d = source.Select(x => KeyValuePair.Create(x.ToString(), x.ToString())).ToFrozenDictionary(optimizeForReading: true);
        var h = source.Select(x => KeyValuePair.Create(x, x)).ToFrozenDictionary(optimizeForReading: false);
        var i = source.Select(x => KeyValuePair.Create(x, x.ToString())).ToFrozenDictionary(optimizeForReading: false);
        var j = source.Select(x => KeyValuePair.Create(x.ToString(), x)).ToFrozenDictionary(optimizeForReading: false);
        var k = source.Select(x => KeyValuePair.Create(x.ToString(), x.ToString())).ToFrozenDictionary(optimizeForReading: false);
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
        yield return new object[] { d };
        yield return new object[] { h };
        yield return new object[] { i };
        yield return new object[] { j };
        yield return new object[] { k };
    }

    [Theory(DisplayName = "Frozen Dictionary Force Base Type Converter")]
    [MemberData(nameof(FrozenDictionaryData))]
    public void FrozenDictionaryForceBaseTypeConverter<T>(T source)
    {
        Assert.NotNull(source);
        var invoke = MakeDelegate<T>(typeof(IReadOnlyDictionary<,>), FrozenDictionaryForceBaseTypeConverterInternal<object, object>);
        invoke.Invoke(source);
    }

    [Theory(DisplayName = "Frozen Dictionary Subclass Type Converter")]
    [MemberData(nameof(FrozenDictionaryData))]
    public void FrozenDictionary<T>(T source)
    {
        Assert.NotNull(source);
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterGenericArgument = Converter.GetGenericArgument(converter);
        var expectedBaseType = typeof(FrozenDictionary<,>).MakeGenericType(converterGenericArgument.GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
            .GetGenericArguments());
        Assert.NotEqual(expectedBaseType, converterGenericArgument);
        Assert.True(converterGenericArgument.IsSubclassOf(expectedBaseType));
        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(Array.Empty<byte>()));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
    }

    public static IEnumerable<object[]> FrozenSetData()
    {
        var source = Enumerable.Range(0, 100).ToList();
        var a = source.ToFrozenSet(optimizeForReading: true);
        var b = source.Select(x => x.ToString()).ToFrozenSet(optimizeForReading: true);
        var h = source.ToFrozenSet(optimizeForReading: false);
        var i = source.Select(x => x.ToString()).ToFrozenSet(optimizeForReading: false);
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { h };
        yield return new object[] { i };
    }

    [Theory(DisplayName = "Frozen Set Force Base Type Converter")]
    [MemberData(nameof(FrozenSetData))]
    public void FrozenSetForceBaseTypeConverter<T>(T source)
    {
        Assert.NotNull(source);
        var invoke = MakeDelegate<T>(typeof(IReadOnlyCollection<>), FrozenSetForceBaseTypeConverterInternal<object>);
        invoke.Invoke(source);
    }

    [Theory(DisplayName = "Frozen Set Subclass Type Converter")]
    [MemberData(nameof(FrozenSetData))]
    public void FrozenSet<T>(T source)
    {
        Assert.NotNull(source);
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterGenericArgument = Converter.GetGenericArgument(converter);
        var expectedBaseType = typeof(FrozenSet<>).MakeGenericType(converterGenericArgument.GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
            .GetGenericArguments());
        Assert.NotEqual(expectedBaseType, converterGenericArgument);
        Assert.True(converterGenericArgument.IsSubclassOf(expectedBaseType));
        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(Array.Empty<byte>()));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
    }
}
