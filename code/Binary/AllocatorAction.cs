namespace Mikodev.Binary;

#if NET9_0_OR_GREATER
public delegate void AllocatorAction<in T>(ref Allocator allocator, scoped T data) where T : allows ref struct;
#else
public delegate void AllocatorAction<in T>(ref Allocator allocator, T data);
#endif
