namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
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
        var source = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, E>(ref MemoryMarshal.GetArrayDataReference(origin)), origin.Length);
        var converter = this.converter;
        foreach (var i in source)
            converter.EncodeAuto(ref allocator, i);
        return;
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        static void Ensure(ReadOnlySpan<int> lengths, int converterLength, int remainingLength)
        {
            Debug.Assert(remainingLength >= 0);
            Debug.Assert(converterLength >= 0);
            var arrayLength = lengths[0];
            for (var i = 1; i < lengths.Length; i++)
                arrayLength = checked(arrayLength * lengths[i]);
            var maxPossibleArrayLength = converterLength is 0 ? remainingLength : (remainingLength / converterLength);
            if (arrayLength <= maxPossibleArrayLength)
                return;
            ThrowHelper.ThrowNotEnoughBytes();
        }

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
        var converter = this.converter;
        Ensure(lengthList, converter.Length, intent.Length);
#if NET9_0_OR_GREATER
        var result = Array.CreateInstanceFromArrayType(typeof(T), lengthList, startsList);
#else
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode")]
        static Array Create(int[] lengths, int[] lowerBounds) => Array.CreateInstance(typeof(E), lengths, lowerBounds);
        var result = Create(lengthList, startsList);
#endif
        var target = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, E>(ref MemoryMarshal.GetArrayDataReference(result)), result.Length);
        for (var i = 0; i < target.Length; i++)
            target[i] = converter.DecodeAuto(ref intent);
        return (T)(object)result;
    }
}
