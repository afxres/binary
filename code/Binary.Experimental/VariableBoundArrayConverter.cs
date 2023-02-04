namespace Mikodev.Binary.Experimental;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed class VariableBoundArrayConverter<T, E> : Converter<T?> where T : class
{
    /* Variable Bound Array Converter
     * Layout: lower bound for all ranks | length for all ranks | Array data ...
     */

    private readonly int rank;

    private readonly Converter<E> converter;

    public VariableBoundArrayConverter(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var type = typeof(T);
        if (type.IsVariableBoundArray is false)
            throw new ArgumentException($"Require variable bound array type, type: {type}");
        var rank = type.GetArrayRank();
        this.rank = rank;
        this.converter = converter;
    }

    private static int GetTotalItemCount(scoped ReadOnlySpan<int> lengthList)
    {
        var result = 1;
        foreach (var i in lengthList)
            result = checked(result * i);
        return result;
    }

    private static Span<E> GetArrayDataSpan(scoped ReadOnlySpan<int> lengthList, Array item)
    {
        var length = GetTotalItemCount(lengthList);
        if (length is 0)
            return default;
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, E>(ref MemoryMarshal.GetArrayDataReference(item)), length);
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        var rank = this.rank;
        var startsList = (stackalloc int[rank]);
        var lengthList = (stackalloc int[rank]);
        var origin = (Array)(object)item;
        for (var i = 0; i < rank; i++)
            Converter.Encode(ref allocator, startsList[i] = origin.GetLowerBound(i));
        for (var i = 0; i < rank; i++)
            Converter.Encode(ref allocator, lengthList[i] = origin.GetLength(i));
        var source = GetArrayDataSpan(lengthList, origin);
        var converter = this.converter;
        foreach (var i in source)
            converter.EncodeAuto(ref allocator, i);
        return;
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var rank = this.rank;
        var startsList = new int[rank];
        var lengthList = new int[rank];
        var intent = span;
        for (var i = 0; i < rank; i++)
            startsList[i] = Converter.Decode(ref intent);
        for (var i = 0; i < rank; i++)
            lengthList[i] = Converter.Decode(ref intent);
        var item = Array.CreateInstance(typeof(E), lengthList, startsList);
        var target = GetArrayDataSpan(lengthList, item);
        var converter = this.converter;
        for (var i = 0; i < target.Length; i++)
            target[i] = converter.DecodeAuto(ref intent);
        return (T)(object)item;
    }
}
