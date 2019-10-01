﻿using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Delegates
{
    internal delegate void OfNamedObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToNamedObject<out T>(in LengthList list);

    internal delegate T ToCollection<out T, in E>(IEnumerable<E> enumerable);

    internal delegate T ToDictionary<out T, K, V>(IDictionary<K, V> dictionary);

    internal delegate T ToValueWith<out T>(ref ReadOnlySpan<byte> span);

    internal delegate void ToBytesWith<in T>(ref Allocator allocator, T item);

    internal delegate E[] ToArray<in T, E>(T collection) where T : IEnumerable<E>;

    internal delegate T[] OfList<T>(List<T> list);

    internal delegate List<T> ToList<T>(T[] buffer, int length);

    internal delegate void OfUnion<in T>(ref Allocator allocator, T item, ref int mark);

    internal delegate T ToUnion<out T>(ref ReadOnlySpan<byte> span, ref int mark);
}
