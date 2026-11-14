namespace Mikodev.Binary.SourceGeneration.Internal;

using System;
using System.Text;

public static class StringBuilderExtensions
{
    public const int IndentSize = 4;

    public const int MaxIndentLevels = 16;

    public static void AppendEmptyLine(this StringBuilder target)
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
        AppendIndent(target, indent, head, tail, count, _ => true, func);
    }

    public static void AppendIndent(this StringBuilder target, int indent, string head, string tail, int count, Func<int, bool> filter, Func<int, string> func)
    {
        if ((uint)indent > MaxIndentLevels)
            throw new ArgumentOutOfRangeException(nameof(indent));
        _ = target.Append(' ', IndentSize * indent);
        _ = target.Append(head);
        var flag = false;
        for (var i = 0; i < count; i++)
        {
            if (filter.Invoke(i) is false)
                continue;
            if (flag)
                _ = target.Append(", ");
            _ = target.Append(func.Invoke(i));
            flag = true;
        }
        _ = target.Append(tail);
        _ = target.AppendLine();
    }
}
