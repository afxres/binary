using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Delegates
{
    internal delegate void OfNamedObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToNamedObject<out T>(in LengthList list);

    internal delegate TCollection ToCollection<out TCollection, in T>(IEnumerable<T> enumerable);

    internal delegate TDictionary ToDictionary<out TDictionary, TIndex, TValue>(IDictionary<TIndex, TValue> dictionary);

    internal delegate T ToValueWith<out T>(ref ReadOnlySpan<byte> span);

    internal delegate void ToBytesWith<in T>(ref Allocator allocator, T item);

    internal delegate T[] ToArray<in TCollection, T>(TCollection collection) where TCollection : IEnumerable<T>;

    internal delegate T[] GetListItems<T>(List<T> list);

    internal delegate void SetListItems<T>(List<T> list, T[] array);

    internal delegate void OfUnion<in T>(ref Allocator allocator, T item, ref int mark);

    internal delegate T ToUnion<out T>(ref ReadOnlySpan<byte> span, ref int mark);
}
