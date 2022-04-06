namespace Mikodev.Binary;

public delegate int AllocatorWriter<in T>(System.Span<byte> span, T data);
