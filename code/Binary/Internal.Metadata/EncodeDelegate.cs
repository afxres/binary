namespace Mikodev.Binary.Internal.Metadata;

internal delegate void EncodeDelegate<in T>(ref Allocator allocator, T item);
