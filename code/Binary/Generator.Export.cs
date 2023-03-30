namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Decoders;
using Mikodev.Binary.Internal.Sequence.Encoders;
using Mikodev.Binary.Internal.SpanLike.Decoders;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;

public static partial class Generator
{
    public static Converter<E> GetEnumConverter<E>() where E : unmanaged
    {
        static Converter<E> Invoke(bool native)
        {
            if (typeof(E).IsEnum is false)
                throw new ArgumentException($"Require an enumeration type!");
            return native
                ? new NativeEndianConverter<E>()
                : new LittleEndianConverter<E>();
        }

        return Invoke(BitConverter.IsLittleEndian);
    }

    public static Converter<E[]> GetArrayConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetArrayConverter(converter);
    }

    public static Converter<ArraySegment<E>> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetArraySegmentConverter(converter);
    }

    public static Converter<ImmutableArray<E>> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetImmutableArrayConverter(converter);
    }

    public static Converter<List<E>> GetListConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetListConverter(converter);
    }

    public static Converter<Memory<E>> GetMemoryConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetMemoryConverter(converter);
    }

    public static Converter<ReadOnlyMemory<E>> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return FallbackSequentialMethods.GetReadOnlyMemoryConverter(converter);
    }

    public static Converter<KeyValuePair<K, V>> GetKeyValuePairConverter<K, V>(Converter<K> key, Converter<V> value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        return new KeyValuePairConverter<K, V>(key, value);
    }

    public static Converter<LinkedList<E>> GetLinkedListConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return new LinkedListConverter<E>(converter);
    }

    public static Converter<T?> GetNullableConverter<T>(Converter<T> converter) where T : struct
    {
        ArgumentNullException.ThrowIfNull(converter);
        return new NullableConverter<T>(converter);
    }

    public static Converter<PriorityQueue<E, P>> GetPriorityQueueConverter<E, P>(Converter<E> element, Converter<P> priority)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(priority);
        return new PriorityQueueConverter<E, P>(element, priority);
    }

    public static Converter<ReadOnlySequence<E>> GetReadOnlySequenceConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return new ReadOnlySequenceConverter<E>(converter);
    }

    public static Converter<HashSet<E>> GetHashSetConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return new SequenceConverter<HashSet<E>>(new HashSetEncoder<E>(converter).Encode, new HashSetDecoder<E>(converter).Decode);
    }

    public static Converter<Dictionary<K, V>> GetDictionaryConverter<K, V>(Converter<K> key, Converter<V> value) where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        var encoder = new DictionaryEncoder<K, V>(key, value);
        var decoder = new DictionaryDecoder<K, V>(key, value);
        return new SequenceConverter<Dictionary<K, V>>(encoder.Encode, decoder.Decode);
    }

    public static Converter<T> GetEnumerableConverter<T, E>(Converter<E> converter, Func<List<E>, T> constructor) where T : IEnumerable<E>
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(constructor);
        var encoder = new EnumerableEncoder<T, E>(converter);
        var decoder = new ListDecoder<E>(converter);
        return new SequenceConverter<T>(encoder.Encode, span => constructor.Invoke(decoder.Invoke(span)));
    }

    public static Converter<T> GetEnumerableConverter<T, E>(Converter<E> converter, Func<HashSet<E>, T> constructor) where T : IEnumerable<E>
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(constructor);
        var encoder = new EnumerableEncoder<T, E>(converter);
        var decoder = new HashSetDecoder<E>(converter);
        return new SequenceConverter<T>(encoder.Encode, span => constructor.Invoke(decoder.Decode(span)));
    }

    public static Converter<T> GetEnumerableConverter<T, K, V>(Converter<K> key, Converter<V> value, Func<List<KeyValuePair<K, V>>, T> constructor) where T : IEnumerable<KeyValuePair<K, V>> where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(constructor);
        var encoder = new KeyValueEnumerableEncoder<T, K, V>(key, value);
        var decoder = new KeyValueEnumerableDecoder<K, V>(key, value);
        return new SequenceConverter<T>(encoder.Encode, span => constructor.Invoke(decoder.Decode(span)));
    }

    public static Converter<T> GetEnumerableConverter<T, K, V>(Converter<K> key, Converter<V> value, Func<Dictionary<K, V>, T> constructor) where T : IEnumerable<KeyValuePair<K, V>> where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(constructor);
        var encoder = new KeyValueEnumerableEncoder<T, K, V>(key, value);
        var decoder = new DictionaryDecoder<K, V>(key, value);
        return new SequenceConverter<T>(encoder.Encode, span => constructor.Invoke(decoder.Decode(span)));
    }
}
