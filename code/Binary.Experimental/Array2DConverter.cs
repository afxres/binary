namespace Mikodev.Binary.Experimental;

using System;

public sealed class Array2DConverter<E> : MultidimensionalArrayConverter<E[,], E, Array2DConverter<E>.Adapter>
{
    public Array2DConverter(Converter<E> converter) : base(converter) { }

    public readonly struct Adapter : IAdapter
    {
        public static E[,] NewArray(Span<int> sizes) => new E[sizes[0], sizes[1]];

        public static E GetValue(E[,] array, Span<int> indexes) => array[indexes[0], indexes[1]];

        public static void SetValue(E[,] array, Span<int> indexes, E item) => array[indexes[0], indexes[1]] = item;
    }
}
