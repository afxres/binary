namespace Mikodev.Binary.SourceGeneration;

using System;
using System.Collections.Generic;
using System.Text;

public static class SystemExtensions
{
    public const int IndentSize = 4;

    public const int MaxIndentLevels = 16;

    public static void AppendIndent(this StringBuilder builder)
    {
        _ = builder.AppendLine();
    }

    public static void AppendIndent(this StringBuilder builder, int indent, string line)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = builder.Append(' ', IndentSize * indent);
        _ = builder.Append(line);
        _ = builder.AppendLine();
    }

    public static void AppendIndent(this StringBuilder builder, int indent, string head, string tail, int count, Func<int, string> func)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = builder.Append(' ', IndentSize * indent);
        _ = builder.Append(head);
        for (var i = 0; i < count; i++)
        {
            var part = func.Invoke(i);
            _ = builder.Append(part);
            if (i == count - 1)
                continue;
            _ = builder.Append(", ");
        }
        _ = builder.Append(tail);
        _ = builder.AppendLine();
    }

    public static bool TryAdd<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}
