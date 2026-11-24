using System.Text;
using System.Xml;

// XML layout for MSBuild project files and .slnx solutions.
//
// dotnet format only handles C# and VB, so nothing else checks csproj, fsproj,
// props, targets or slnx.
//
// usage:
//   dotnet run scripts/format-projects.cs -- <files...>          # check, exit 1 on differences
//   dotnet run scripts/format-projects.cs -- --fix <files...>    # rewrite in place
//
// exit codes: 0 clean, 1 needs formatting, 2 unreadable or malformed XML.
//
// Layout: two space indent, attributes never wrapped, self-closing <X />,
// blank lines between siblings kept, comment bodies untouched, LF endings,
// trailing newline, byte order mark kept.

var fix = args.Contains("--fix");
var files = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();

if (args.Any(a => a is "-h" or "--help"))
{
    Console.WriteLine("usage: dotnet run scripts/format-projects.cs -- [--fix] <files...>");
    return 0;
}

foreach (var unknown in args.Where(a => a.StartsWith("--", StringComparison.Ordinal) && a != "--fix"))
{
    Console.Error.WriteLine($"error: unknown argument: {unknown}");
    return 2;
}

if (files.Length == 0)
{
    Console.Error.WriteLine("usage: dotnet run scripts/format-projects.cs -- [--fix] <files...>");
    return 2;
}

var changed = 0;
var failed = 0;
foreach (var file in files)
{
    string original;
    bool hasBom;
    try
    {
        var bytes = File.ReadAllBytes(file);
        hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        original = Encoding.UTF8.GetString(bytes, hasBom ? 3 : 0, bytes.Length - (hasBom ? 3 : 0));
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"{file}: {ex.Message}");
        failed++;
        continue;
    }

    string formatted;
    try
    {
        formatted = Layout.Format(original);
    }
    catch (XmlException ex)
    {
        Console.Error.WriteLine($"{file}: invalid XML at line {ex.LineNumber}, position {ex.LinePosition}: {ex.Message}");
        failed++;
        continue;
    }

    if (formatted == original)
    {
        continue;
    }

    changed++;
    if (fix)
    {
        var bytes = Encoding.UTF8.GetBytes(formatted);
        File.WriteAllBytes(file, hasBom ? [0xEF, 0xBB, 0xBF, .. bytes] : bytes);
        Console.WriteLine($"fixed {file}");
    }
    else
    {
        Console.WriteLine(file);
    }
}

if (failed > 0)
{
    return 2;
}

if (changed > 0 && !fix)
{
    Console.Error.WriteLine($"{changed} file(s) need formatting, run: scripts/check-format.sh --projects --fix");
    return 1;
}

return 0;

internal static class Layout
{
    public static string Format(string xml)
    {
        // PreserveWhitespace keeps the text nodes that carry the authored blank lines.
        var doc = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using (var reader = XmlReader.Create(
                   new StringReader(xml),
                   new XmlReaderSettings { IgnoreWhitespace = false, DtdProcessing = DtdProcessing.Ignore, XmlResolver = null }))
        {
            doc.Load(reader);
        }

        var sb = new StringBuilder(xml.Length);
        foreach (XmlNode node in doc.ChildNodes)
        {
            if (IsBlank(node))
            {
                continue;
            }

            if (HasBlankLineBefore(node))
            {
                sb.Append('\n');
            }

            WriteNode(node, 0, sb);
        }

        return sb.ToString();
    }

    private static void WriteNode(XmlNode node, int depth, StringBuilder sb)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Element:
                WriteElement((XmlElement)node, depth, sb);
                break;
            case XmlNodeType.Comment:
                // Bodies are not reflowed, only the opening line is re-indented.
                sb.Append(Pad(depth)).Append("<!--").Append(node.Value).Append("-->\n");
                break;
            case XmlNodeType.XmlDeclaration:
                sb.Append("<?xml").Append(string.IsNullOrEmpty(node.Value) ? string.Empty : " " + node.Value).Append(" ?>\n");
                break;
            case XmlNodeType.ProcessingInstruction:
                sb.Append(Pad(depth)).Append("<?").Append(node.Name).Append(' ').Append(node.Value).Append(" ?>\n");
                break;
            case XmlNodeType.CDATA:
                sb.Append(Pad(depth)).Append("<![CDATA[").Append(node.Value).Append("]]>\n");
                break;
            case XmlNodeType.Text:
            case XmlNodeType.SignificantWhitespace:
                // Mixed content is emitted as authored.
                sb.Append(EscapeText(node.Value ?? string.Empty));
                break;
            default:
                break;
        }
    }

    private static void WriteElement(XmlElement element, int depth, StringBuilder sb)
    {
        sb.Append(Pad(depth)).Append('<').Append(element.Name);

        foreach (XmlAttribute attribute in element.Attributes)
        {
            sb.Append(' ').Append(attribute.Name).Append("=\"").Append(EscapeAttribute(attribute.Value)).Append('"');
        }

        var children = element.ChildNodes.Cast<XmlNode>().Where(n => !IsBlank(n)).ToArray();
        if (children.Length == 0)
        {
            sb.Append(" />\n");
            return;
        }

        // A single text child stays on the opening line, e.g. <Nullable>enable</Nullable>.
        if (children.All(n => n.NodeType is XmlNodeType.Text or XmlNodeType.CDATA))
        {
            sb.Append('>');
            foreach (var child in children)
            {
                sb.Append(EscapeText(child.InnerText));
            }

            sb.Append("</").Append(element.Name).Append(">\n");
            return;
        }

        sb.Append(">\n");
        foreach (var child in children)
        {
            if (HasBlankLineBefore(child))
            {
                sb.Append('\n');
            }

            WriteNode(child, depth + 1, sb);
        }

        sb.Append(Pad(depth)).Append("</").Append(element.Name).Append(">\n");
    }

    private static bool IsBlank(XmlNode node) =>
        node.NodeType is XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace;

    private static bool HasBlankLineBefore(XmlNode node) =>
        node.PreviousSibling is { NodeType: XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace } previous
        && previous.Value is { } text
        && text.AsSpan().Count('\n') >= 2;

    private static string Pad(int depth) => new(' ', depth * 2);

    private static string EscapeAttribute(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '&': sb.Append("&amp;"); break;
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                case '"': sb.Append("&quot;"); break;
                case '\n': sb.Append("&#xA;"); break;
                case '\r': sb.Append("&#xD;"); break;
                case '\t': sb.Append("&#x9;"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }

    private static string EscapeText(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '&': sb.Append("&amp;"); break;
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }
}
