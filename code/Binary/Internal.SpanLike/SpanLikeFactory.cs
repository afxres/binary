namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Adapters;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

internal static class SpanLikeFactory
{
    private static Converter<T> GetConverter<T, E, A>(Converter<E> converter) where A : struct, ISpanLikeAdapter<T, E>
    {
        return NativeEndian.IsNativeEndianConverter(converter) ? new ArrayBasedNativeEndianConverter<T, E, A>() : new ArrayBasedConverter<T, E, A>(converter);
    }

    internal static Converter<E[]> GetArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter<E[], E, ArrayAdapter<E>>(converter);
    }

    internal static Converter<ArraySegment<E>> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        return GetConverter<ArraySegment<E>, E, ArraySegmentAdapter<E>>(converter);
    }

    internal static Converter<ImmutableArray<E>> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter<ImmutableArray<E>, E, ImmutableArrayAdapter<E>>(converter);
    }

    internal static Converter<List<E>> GetListConverter<E>(Converter<E> converter)
    {
        return NativeEndian.IsNativeEndianConverter(converter) ? new ListNativeEndianConverter<E>() : new ListConverter<E>(converter);
    }

    internal static Converter<Memory<E>> GetMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter<Memory<E>, E, MemoryAdapter<E>>(converter);
    }

    internal static Converter<ReadOnlyMemory<E>> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter<ReadOnlyMemory<E>, E, ReadOnlyMemoryAdapter<E>>(converter);
    }
}
