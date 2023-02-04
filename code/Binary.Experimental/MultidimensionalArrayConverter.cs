namespace Mikodev.Binary.Experimental;

using System;

public partial class MultidimensionalArrayConverter<T, E, U> : Converter<T?> where T : class where U : struct, MultidimensionalArrayConverter<T, E, U>.IAdapter
{
    /* Multidimensional Array Converter
     * Layout: length of each rank | data
     * Design Issue: if rank is 1, built-in SZ-Array converter does not write array length but this converter will do
     */

    private readonly int rank;

    private readonly Converter<E> converter;

    public MultidimensionalArrayConverter(Converter<E> converter)
    {
        if (typeof(T).IsArray is false)
            throw new ArgumentException($"Require array type, type: {typeof(T)}");
        var rank = typeof(T).GetArrayRank();
        if (rank < 2)
            throw new ArgumentException($"Require 2 for more ranks for array type, type: {typeof(T)}");
        this.rank = rank;
        this.converter = converter;
    }

    private void EncodeInternal(scoped ReadOnlySpan<int> lengthList, scoped Span<int> cursorList, ref Allocator allocator, T array, int depth)
    {
        var length = lengthList[depth];
        ref var i = ref cursorList[depth];
        if (depth == this.rank - 1)
            for (i = 0; i < length; i++)
                this.converter.EncodeAuto(ref allocator, U.GetValue(array, cursorList));
        else
            for (i = 0; i < length; i++)
                EncodeInternal(lengthList, cursorList, ref allocator, array, depth + 1);
        return;
    }

    private void DecodeInternal(scoped ReadOnlySpan<int> lengthList, scoped Span<int> cursorList, ref ReadOnlySpan<byte> span, T array, int depth)
    {
        var length = lengthList[depth];
        ref var i = ref cursorList[depth];
        if (depth == this.rank - 1)
            for (i = 0; i < length; i++)
                U.SetValue(array, cursorList, this.converter.DecodeAuto(ref span));
        else
            for (i = 0; i < length; i++)
                DecodeInternal(lengthList, cursorList, ref span, array, depth + 1);
        return;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        var rank = this.rank;
        var lengthList = (stackalloc int[rank]);
        var cursorList = (stackalloc int[rank]);
        for (var i = 0; i < rank; i++)
            Converter.Encode(ref allocator, lengthList[i] = ((Array)(object)item).GetLength(i));
        EncodeInternal(lengthList, cursorList, ref allocator, item, 0);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var rank = this.rank;
        var intent = span;
        var lengthList = (stackalloc int[rank]);
        var cursorList = (stackalloc int[rank]);
        for (var i = 0; i < rank; i++)
            lengthList[i] = Converter.Decode(ref intent);
        var result = U.NewArray(lengthList);
        DecodeInternal(lengthList, cursorList, ref intent, result, 0);
        return result;
    }
}
