namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts.Decoders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public abstract class NamedObjectConverter<T>(IEnumerable<IEnumerable<byte>> headers, IEnumerable<string> names, IEnumerable<bool> optional) : Converter<T?>
{
    private readonly NamedObjectDecoder invoke = new NamedObjectDecoder([.. (headers ?? []).Select(x => x.ToImmutableArray())], [.. names ?? []], [.. optional ?? []], typeof(T));

    public sealed override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ObjectModule.GetNullValueOrNotEnoughBytes<T>();

        var invoke = this.invoke;
        var slices = (stackalloc long[invoke.Length]);
        invoke.Invoke(span, slices);
        return Decode(new NamedObjectParameter(span, slices));
    }

    public abstract T Decode(scoped NamedObjectParameter parameter);
}
