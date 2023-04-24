namespace Mikodev.Binary.SourceGeneration.CollectionTests.CompatibleTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public interface IAbstractEnumerable<T> : IEnumerable<T> { }

public interface IAbstractDictionary<K, V> : IDictionary<K, V> { }

public interface IAbstractReadOnlyDictionary<K, V> : IReadOnlyDictionary<K, V> { }

public abstract class AbstractEnumerable<T> : IAbstractEnumerable<T>
{
    public IEnumerable<T> Collection { get; }

    public AbstractEnumerable(IEnumerable<T> collection) => Collection = collection;

    public IEnumerator<T> GetEnumerator() => Collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class AbstractDictionary<K, V> : AbstractEnumerable<KeyValuePair<K, V>>, IAbstractDictionary<K, V>
{
    public AbstractDictionary(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }

    void IDictionary<K, V>.Add(K key, V value) => throw new NotImplementedException();

    bool IDictionary<K, V>.ContainsKey(K key) => throw new NotImplementedException();

    bool IDictionary<K, V>.Remove(K key) => throw new NotImplementedException();

    bool IDictionary<K, V>.TryGetValue(K key, out V value) => throw new NotImplementedException();

    V IDictionary<K, V>.this[K key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    ICollection<K> IDictionary<K, V>.Keys => throw new NotImplementedException();

    ICollection<V> IDictionary<K, V>.Values => throw new NotImplementedException();

    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => throw new NotImplementedException();

    void ICollection<KeyValuePair<K, V>>.Clear() => throw new NotImplementedException();

    bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item) => throw new NotImplementedException();

    void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => throw new NotImplementedException();

    bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item) => throw new NotImplementedException();

    int ICollection<KeyValuePair<K, V>>.Count => throw new NotImplementedException();

    bool ICollection<KeyValuePair<K, V>>.IsReadOnly => throw new NotImplementedException();
}

public abstract class AbstractReadOnlyDictionary<K, V> : AbstractEnumerable<KeyValuePair<K, V>>, IAbstractReadOnlyDictionary<K, V>
{
    public AbstractReadOnlyDictionary(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }

    bool IReadOnlyDictionary<K, V>.ContainsKey(K key) => throw new NotImplementedException();

    bool IReadOnlyDictionary<K, V>.TryGetValue(K key, out V value) => throw new NotImplementedException();

    V IReadOnlyDictionary<K, V>.this[K key] => throw new NotImplementedException();

    IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => throw new NotImplementedException();

    IEnumerable<V> IReadOnlyDictionary<K, V>.Values => throw new NotImplementedException();

    int IReadOnlyCollection<KeyValuePair<K, V>>.Count => throw new NotImplementedException();
}

public class CustomEnumerable<T> : AbstractEnumerable<T>
{
    public CustomEnumerable(IEnumerable<T> collection) : base(collection) { }
}

public class CustomEnumerableInternalConstructor<T> : AbstractEnumerable<T>
{
    internal CustomEnumerableInternalConstructor(IEnumerable<T> collection) : base(collection) { }
}

public class CustomDictionary<K, V> : AbstractDictionary<K, V>
{
    public CustomDictionary(IDictionary<K, V> collection) : base(collection) { }
}

public class CustomReadOnlyDictionary<K, V> : AbstractReadOnlyDictionary<K, V>
{
    public CustomReadOnlyDictionary(IReadOnlyDictionary<K, V> collection) : base(collection) { }
}

public class CustomDictionaryEnumerableConstructor<K, V> : AbstractDictionary<K, V>
{
    public CustomDictionaryEnumerableConstructor(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }
}

public class CustomReadOnlyDictionaryEnumerableConstructor<K, V> : AbstractReadOnlyDictionary<K, V>
{
    public CustomReadOnlyDictionaryEnumerableConstructor(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }
}

public class CustomDictionaryInternalConstructor<K, V> : AbstractDictionary<K, V>
{
    internal CustomDictionaryInternalConstructor(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }
}

public class CustomReadOnlyDictionaryInternalConstructor<K, V> : AbstractReadOnlyDictionary<K, V>
{
    internal CustomReadOnlyDictionaryInternalConstructor(IEnumerable<KeyValuePair<K, V>> collection) : base(collection) { }
}

[SourceGeneratorContext]
[SourceGeneratorInclude<List<int>>]
[SourceGeneratorInclude<List<string>>]
[SourceGeneratorInclude<List<KeyValuePair<int, string>>>]
[SourceGeneratorInclude<List<KeyValuePair<string, int>>>]
[SourceGeneratorInclude<CustomEnumerable<int>>]
[SourceGeneratorInclude<CustomEnumerable<string>>]
[SourceGeneratorInclude<CustomEnumerableInternalConstructor<int>>]
[SourceGeneratorInclude<CustomEnumerableInternalConstructor<string>>]
[SourceGeneratorInclude<CustomDictionary<int, string>>]
[SourceGeneratorInclude<CustomReadOnlyDictionary<string, int>>]
[SourceGeneratorInclude<CustomDictionaryEnumerableConstructor<int, string>>]
[SourceGeneratorInclude<CustomReadOnlyDictionaryEnumerableConstructor<string, int>>]
[SourceGeneratorInclude<CustomDictionaryInternalConstructor<int, string>>]
[SourceGeneratorInclude<CustomReadOnlyDictionaryInternalConstructor<string, int>>]
[SourceGeneratorInclude<AbstractEnumerable<int>>]
[SourceGeneratorInclude<AbstractEnumerable<string>>]
[SourceGeneratorInclude<IAbstractEnumerable<int>>]
[SourceGeneratorInclude<IAbstractEnumerable<string>>]
[SourceGeneratorInclude<AbstractDictionary<int, string>>]
[SourceGeneratorInclude<AbstractReadOnlyDictionary<string, int>>]
[SourceGeneratorInclude<IAbstractDictionary<int, string>>]
[SourceGeneratorInclude<IAbstractReadOnlyDictionary<string, int>>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    public static IEnumerable<object[]> EnumerableData()
    {
        var a = Enumerable.Range(0, 100).ToList();
        var b = a.Select(x => x.ToString()).ToList();
        yield return new object[] { new CustomEnumerable<int>(a), a };
        yield return new object[] { new CustomEnumerable<string>(b), b };
    }

    public static IEnumerable<object[]> EnumerableKeyValuePairData()
    {
        var a = new[] { KeyValuePair.Create(0, "1") }.ToList();
        var b = new[] { KeyValuePair.Create("2", 3) }.ToList();
        yield return new object[] { new CustomDictionaryEnumerableConstructor<int, string>(a), a };
        yield return new object[] { new CustomReadOnlyDictionaryEnumerableConstructor<string, int>(b), b };
        yield return new object[] { new CustomDictionary<int, string>(new Dictionary<int, string>(a)), a };
        yield return new object[] { new CustomReadOnlyDictionary<string, int>(new Dictionary<string, int>(b)), b };
    }

    [Theory(DisplayName = "Enumerable Test")]
    [MemberData(nameof(EnumerableData))]
    [MemberData(nameof(EnumerableKeyValuePairData))]
    public void EnumerableTest<T, E>(T source, List<E> actual) where T : IEnumerable<E>
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();
        var converterList = generator.GetConverter<List<E>>();
        var buffer = converter.Encode(source);
        var bufferList = converterList.Encode(actual);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(bufferList, buffer);
        Assert.Equal(bufferList, bufferSecond);

        var result = converter.Decode(buffer);
        var resultSecond = converterSecond.Decode(buffer);
        Assert.Equal(actual, result);
        Assert.Equal(actual, resultSecond);
    }

    public static IEnumerable<object[]> EnumerableEncodeOnlyData()
    {
        var a = Enumerable.Range(0, 100).ToList();
        var b = a.Select(x => x.ToString()).ToList();
        yield return new object[] { new CustomEnumerableInternalConstructor<int>(a), a };
        yield return new object[] { new CustomEnumerableInternalConstructor<string>(b), b };
    }

    public static IEnumerable<object[]> EnumerableKeyValuePairEncodeOnlyData()
    {
        var a = new[] { KeyValuePair.Create(0, "1") }.ToList();
        var b = new[] { KeyValuePair.Create("2", 3) }.ToList();
        yield return new object[] { new CustomDictionaryInternalConstructor<int, string>(a), a };
        yield return new object[] { new CustomReadOnlyDictionaryInternalConstructor<string, int>(b), b };
    }

    [Theory(DisplayName = "Enumerable Encode Only Test")]
    [MemberData(nameof(EnumerableEncodeOnlyData))]
    [MemberData(nameof(EnumerableKeyValuePairEncodeOnlyData))]
    public void EnumerableEncodeOnlyTest<T, E>(T source, List<E> actual) where T : IEnumerable<E>
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();
        var converterList = generator.GetConverter<List<E>>();
        var buffer = converter.Encode(source);
        var bufferList = converterList.Encode(actual);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(bufferList, buffer);
        Assert.Equal(bufferList, bufferSecond);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var errorSecond = Assert.Throws<NotSupportedException>(() => converterSecond.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorSecond.Message);
    }

    public static IEnumerable<object[]> EnumerableInterfaceOrAbstractClassData()
    {
        var a = Enumerable.Range(0, 100).ToList();
        var b = a.Select(x => x.ToString()).ToList();
        yield return new object[] { typeof(AbstractEnumerable<int>), new CustomEnumerable<int>(a), a };
        yield return new object[] { typeof(AbstractEnumerable<string>), new CustomEnumerable<string>(b), b };
        yield return new object[] { typeof(IAbstractEnumerable<int>), new CustomEnumerable<int>(a), a };
        yield return new object[] { typeof(IAbstractEnumerable<string>), new CustomEnumerable<string>(b), b };
    }

    public static IEnumerable<object[]> EnumerableKeyValuePairInterfaceOrAbstractClassData()
    {
        var a = new[] { KeyValuePair.Create(0, "1") }.ToList();
        var b = new[] { KeyValuePair.Create("2", 3) }.ToList();
        yield return new object[] { typeof(AbstractDictionary<int, string>), new CustomDictionary<int, string>(new Dictionary<int, string>(a)), a };
        yield return new object[] { typeof(AbstractReadOnlyDictionary<string, int>), new CustomReadOnlyDictionary<string, int>(new Dictionary<string, int>(b)), b };
        yield return new object[] { typeof(IAbstractDictionary<int, string>), new CustomDictionary<int, string>(new Dictionary<int, string>(a)), a };
        yield return new object[] { typeof(IAbstractReadOnlyDictionary<string, int>), new CustomReadOnlyDictionary<string, int>(new Dictionary<string, int>(b)), b };
    }

    [Theory(DisplayName = "Enumerable Interface Or Abstract Class Test")]
    [MemberData(nameof(EnumerableInterfaceOrAbstractClassData))]
    [MemberData(nameof(EnumerableKeyValuePairInterfaceOrAbstractClassData))]
    public void EnumerableInterfaceOrAbstractClassTest<T, E>(Type wantedType, T source, List<E> actual) where T : IEnumerable<E>
    {
        Assert.True(wantedType.IsInterface || wantedType.IsAbstract);
        var method = new Action<IEnumerable<object>, List<object>>(EnumerableEncodeOnlyTest).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { wantedType, typeof(E) });
        var result = target.Invoke(this, new object?[] { source, actual });
        Assert.Null(result);
    }
}
