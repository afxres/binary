namespace Mikodev.Binary.SourceGeneration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class SystemExtensions
{
    public static void AppendIndent(this StringBuilder builder)
    {
        _ = builder.AppendLine();
    }

    public static void AppendIndent(this StringBuilder builder, int indent, string line)
    {
        const int MaxLoop = 16;
        if ((uint)indent > MaxLoop)
            throw new ArgumentOutOfRangeException(nameof(indent));
        var current = new StringBuilder();
        for (var i = 0; i < indent; i++)
            _ = current.Append("    ");
        _ = current.Append(line);
        _ = builder.Append(current.ToString());
        _ = builder.AppendLine();
    }

    public static void AppendIndent(this StringBuilder builder, int indent, string head, string tail, int count, Func<int, string> func)
    {
        AppendIndent(builder, indent, head, tail, Enumerable.Range(0, count).ToList(), func);
    }

    public static void AppendIndent<T>(this StringBuilder builder, int indent, string head, string tail, IReadOnlyList<T> values, Func<T, string> func)
    {
        const int MaxLoop = 16;
        if ((uint)indent > MaxLoop)
            throw new ArgumentOutOfRangeException(nameof(indent));
        var current = new StringBuilder();
        for (var i = 0; i < indent; i++)
            _ = current.Append("    ");
        _ = current.Append(head);
        for (var i = 0; i < values.Count; i++)
        {
            var part = func.Invoke(values[i]);
            _ = current.Append(part);
            if (i == values.Count - 1)
                continue;
            _ = current.Append(", ");
        }
        _ = current.Append(tail);
        _ = builder.Append(current.ToString());
        _ = builder.AppendLine();
    }
}
