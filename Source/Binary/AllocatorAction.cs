using System;

namespace Mikodev.Binary
{
    public delegate void AllocatorAction<in T>(Span<byte> span, T data);
}
