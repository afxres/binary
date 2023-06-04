namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public abstract class NamedObjectConverter<T> : Converter<T?>
{
    private readonly NamedObjectDecoder invoke;

    public NamedObjectConverter(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(optional);
        this.invoke = NamedObjectDecoder.Create(typeof(T), converter, names.ToImmutableArray(), optional.ToImmutableArray());
    }

    public abstract T Decode(scoped NamedObjectParameter parameter);

    public sealed override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ObjectModule.GetNullValueOrThrow<T>();

        // maybe 'StackOverflowException', just let it crash
        var invoke = this.invoke;
        var slices = (stackalloc long[invoke.MemberLength]);
        invoke.Invoke(span, slices);
        return Decode(new NamedObjectParameter(span, slices));
    }
}
