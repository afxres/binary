namespace Mikodev.Binary.Experimental;

using System;

public partial class MultidimensionalArrayConverter<T, E, U>
{
    public interface IAdapter
    {
        static abstract T NewArray(Span<int> sizes);

        static abstract E GetValue(T array, Span<int> indexes);

        static abstract void SetValue(T array, Span<int> indexes, E item);
    }
}
