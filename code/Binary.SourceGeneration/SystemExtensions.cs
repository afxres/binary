namespace Mikodev.Binary.SourceGeneration;

using System;
using System.Collections.Generic;
using System.Linq;
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
        AppendIndent(builder, indent, head, tail, Enumerable.Range(0, count).ToList(), func);
    }

    public static void AppendIndent<T>(this StringBuilder builder, int indent, string head, string tail, IReadOnlyList<T> values, Func<T, string> func)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = builder.Append(' ', IndentSize * indent);
        _ = builder.Append(head);
        for (var i = 0; i < values.Count; i++)
        {
            var part = func.Invoke(values[i]);
            _ = builder.Append(part);
            if (i == values.Count - 1)
                continue;
            _ = builder.Append(", ");
        }
        _ = builder.Append(tail);
        _ = builder.AppendLine();
    }
}
