namespace Mikodev.Binary;

public delegate int AllocatorWriter<in T>(scoped System.Span<byte> span, T data);
