namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.External;
using System;
using System.Collections.Immutable;
using System.Linq;

internal sealed partial class NamedObjectDecoder
{
    internal static NamedObjectDecoder Create(Type type, Converter<string> converter, ImmutableArray<string> names, ImmutableArray<bool> optional)
    {
        if (names.Length is 0 || optional.Length is 0)
            throw new ArgumentException($"Sequence contains no element.");
        if (names.Length != optional.Length)
            throw new ArgumentException($"Sequence lengths not match.");
        var selector = new Func<string, ReadOnlyMemory<byte>>(x => converter.Encode(x));
        var dictionary = BinaryObject.Create(names.Select(selector).ToImmutableArray());
        if (dictionary is null)
            throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {converter.GetType()}");
        return new NamedObjectDecoder(type, names, optional, dictionary);
    }
}
