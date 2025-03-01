namespace Mikodev.Binary;

public delegate void AllocatorAction<in T>(ref Allocator allocator, T data);
