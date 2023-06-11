﻿namespace Mikodev.Binary.SourceGeneration.Internal;

using System;
using System.Collections.Generic;
using System.Text;

public static class SystemExtensions
{
    public const int IndentSize = 4;

    public const int MaxIndentLevels = 16;

    public static void AppendIndent(this StringBuilder target)
    {
        _ = target.AppendLine();
    }

    public static void AppendIndent(this StringBuilder target, int indent, string line)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = target.Append(' ', IndentSize * indent);
        _ = target.Append(line);
        _ = target.AppendLine();
    }

    public static void AppendIndent(this StringBuilder target, int indent, string head, string tail, int count, Func<int, string> func)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = target.Append(' ', IndentSize * indent);
        _ = target.Append(head);
        for (var i = 0; i < count; i++)
        {
            var part = func.Invoke(i);
            _ = target.Append(part);
            if (i == count - 1)
                continue;
            _ = target.Append(", ");
        }
        _ = target.Append(tail);
        _ = target.AppendLine();
    }

    public static bool TryAdd<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}
