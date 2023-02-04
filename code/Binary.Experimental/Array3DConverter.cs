namespace Mikodev.Binary.Experimental;

using System;

public sealed class Array3DConverter<E> : MultidimensionalArrayConverter<E[,,], E, Array3DConverter<E>.Adapter>
{
    public Array3DConverter(Converter<E> converter) : base(converter) { }

    public readonly struct Adapter : IAdapter
    {
        public static E[,,] NewArray(Span<int> sizes) => new E[sizes[0], sizes[1], sizes[2]];

        public static E GetValue(E[,,] array, Span<int> indexes) => array[indexes[0], indexes[1], indexes[2]];

        public static void SetValue(E[,,] array, Span<int> indexes, E item) => array[indexes[0], indexes[1], indexes[2]] = item;
    }
}
