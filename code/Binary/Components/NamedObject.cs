namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Contexts.Template;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public static class NamedObject
{
    public static Converter<T?> GetNamedObjectConverter<T>(AllocatorAction<T> action, NamedObjectConstructor<T>? constructor, Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(optional);
        var alpha = names.ToImmutableArray();
        var bravo = optional.ToImmutableArray();
        if (alpha.Length is 0 || bravo.Length is 0)
            throw new ArgumentException($"Collection is empty.");
        if (alpha.Length != bravo.Length)
            throw new ArgumentException($"Collection lengths not match.");
        var dictionary = NamedObjectArguments.GetDictionary(typeof(T), converter, alpha);
        return new NamedObjectConverter<T>(action, constructor, dictionary, alpha, bravo);
    }
}
