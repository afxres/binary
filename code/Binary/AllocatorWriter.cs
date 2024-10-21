namespace Mikodev.Binary;

#if NET9_0_OR_GREATER
public delegate int AllocatorWriter<in T>(scoped System.Span<byte> span, T data) where T : allows ref struct;
#else
public delegate int AllocatorWriter<in T>(scoped System.Span<byte> span, T data);
#endif
