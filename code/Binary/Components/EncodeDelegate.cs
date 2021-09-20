namespace Mikodev.Binary.Components;

internal delegate void EncodeDelegate<in T>(ref Allocator allocator, T item);
