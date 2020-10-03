#if NETOLD

using System;

namespace Mikodev.Binary
{
    public delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);
}

#endif
