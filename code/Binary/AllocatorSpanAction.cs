namespace Mikodev.Binary;

public delegate int AllocatorSpanAction<in T>(System.Span<byte> span, T data);
