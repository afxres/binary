﻿namespace Mikodev.Binary;

using Mikodev.Binary.Creators;
using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Decoders;
using Mikodev.Binary.Internal.Sequence.Encoders;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;

public static partial class Generator
{
    public static Converter<E> GetEnumConverter<E>() where E : unmanaged
    {
        if (typeof(E).IsEnum is false)
            throw new ArgumentException($"Require an enumeration type!");
        return new LittleEndianConverter<E>();
    }

    public static Converter<T?> GetVariableBoundArrayConverter<T, E>(Converter<E> converter) where T : class
    {
        ArgumentNullException.ThrowIfNull(converter);
        if (typeof(T).IsVariableBoundArray is false)
            throw new ArgumentException("Require variable bound array type.");
        if (typeof(T).GetElementType() != typeof(E))
            throw new ArgumentException("Element type not match.");
        return new VariableBoundArrayConverter<T, E>(converter);
    }

    public static Converter<E[]> GetArrayConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetArrayConverter(converter);
    }

    public static Converter<ArraySegment<E>> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetArraySegmentConverter(converter);
    }

    public static Converter<ImmutableArray<E>> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetImmutableArrayConverter(converter);
    }

    public static Converter<List<E>> GetListConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetListConverter(converter);
    }

    public static Converter<Memory<E>> GetMemoryConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetMemoryConverter(converter);
    }

    public static Converter<ReadOnlyMemory<E>> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        return SpanLikeFactory.GetReadOnlyMemoryConverter(converter);
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
        var encoder = new HashSetEncoder<E>(converter);
        var decoder = new HashSetDecoder<E>(converter);
        return new SequenceConverter<HashSet<E>>(encoder.Encode, decoder.Invoke);
    }

    public static Converter<Dictionary<K, V>> GetDictionaryConverter<K, V>(Converter<K> key, Converter<V> value) where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        var encoder = new DictionaryEncoder<K, V>(key, value);
        var decoder = new DictionaryDecoder<K, V>(key, value);
        return new SequenceConverter<Dictionary<K, V>>(encoder.Encode, decoder.Invoke);
    }
}
