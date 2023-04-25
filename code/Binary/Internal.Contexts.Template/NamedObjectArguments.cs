namespace Mikodev.Binary.Internal.Contexts.Template;

using Mikodev.Binary.External;
using System;
using System.Collections.Immutable;
using System.Linq;

internal static class NamedObjectArguments
{
    internal static ByteViewDictionary<int> GetDictionary(Type type, Converter<string> converter, ImmutableArray<string> names)
    {
        var selector = new Func<string, ReadOnlyMemory<byte>>(x => converter.Encode(x));
        var dictionary = BinaryObject.Create(names.Select(selector).ToImmutableArray());
        if (dictionary is null)
            throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {converter.GetType()}");
        return dictionary;
    }
}
