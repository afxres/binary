using System;

namespace Mikodev.Binary
{
    public delegate void AllocatorAction<T>(Span<byte> span, T data);
}
