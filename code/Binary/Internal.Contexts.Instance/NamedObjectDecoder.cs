namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.External;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

internal sealed partial class NamedObjectDecoder
{
    private readonly Type type;

    private readonly int required;

    private readonly int capacity;

    private readonly ImmutableArray<bool> optional;

    private readonly ImmutableArray<string> names;

    private readonly ByteViewDictionary<int> dictionary;

    private NamedObjectDecoder(Type type, ImmutableArray<string> names, ImmutableArray<bool> optional, ByteViewDictionary<int> dictionary)
    {
        Debug.Assert(names.Length is not 0);
        Debug.Assert(names.Length == optional.Length);
        this.type = type;
        this.names = names;
        this.optional = optional;
        this.capacity = optional.Length;
        this.required = optional.Count(x => x is false);
        this.dictionary = dictionary;
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptKeyFound(int cursor)
    {
        throw new ArgumentException($"Named key '{this.names[cursor]}' already exists, type: {this.type}");
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptNotFound(ReadOnlySpan<long> span)
    {
        var cursor = -1;
        var optional = this.optional;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] is not 0 || optional[i])
                continue;
            cursor = i;
            break;
        }
        throw new ArgumentException($"Named key '{this.names[cursor]}' does not exist, type: {this.type}");
    }
}
