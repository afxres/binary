﻿namespace Mikodev.Binary.Creators;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class VariableBoundArrayConverter<T, E> : Converter<T?> where T : class
{
    /* Variable Bound Array Converter
     * Layout: length for all ranks | lower bound for all ranks | array data ...
     */

    private readonly int rank;

    private readonly Converter<E> converter;

    public VariableBoundArrayConverter(Converter<E> converter)
    {
        Debug.Assert(converter is not null);
        Debug.Assert(typeof(T).IsVariableBoundArray);
        var rank = typeof(T).GetArrayRank();
        this.rank = rank;
        this.converter = converter;
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
            Converter.Encode(ref allocator, lengthList[i] = origin.GetLength(i));
        for (var i = 0; i < rank; i++)
            Converter.Encode(ref allocator, startsList[i] = origin.GetLowerBound(i));
        var source = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, E>(ref MemoryMarshal.GetArrayDataReference(origin)), origin.Length);
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
            lengthList[i] = Converter.Decode(ref intent);
        for (var i = 0; i < rank; i++)
            startsList[i] = Converter.Decode(ref intent);
        var result = Array.CreateInstance(typeof(E), lengthList, startsList);
        var target = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, E>(ref MemoryMarshal.GetArrayDataReference(result)), result.Length);
        var converter = this.converter;
        for (var i = 0; i < target.Length; i++)
            target[i] = converter.DecodeAuto(ref intent);
        return (T)(object)result;
    }
}
