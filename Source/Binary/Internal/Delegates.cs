using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal
{
    internal delegate void OfTupleObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToTupleObject<out T>(ref ReadOnlySpan<byte> span);

    internal delegate void OfNamedObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToNamedObject<out T>(LengthList list);

    internal delegate T[] OfList<T>(List<T> list);

    internal delegate List<T> ToList<T>(T[] buffer, int length);

    internal delegate E[] ToArray<in T, out E>(T collection) where T : IEnumerable<E>;

    internal delegate T ToCollection<out T, in E>(IEnumerable<E> enumerable);

    internal delegate T ToDictionary<out T, K, V>(IDictionary<K, V> dictionary);
}
