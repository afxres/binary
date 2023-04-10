namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

public class SymbolTypeAliases
{
    private readonly SortedDictionary<string, (string, string?)> aliases = new SortedDictionary<string, (string, string?)>();

    public void Add(string type, string alias, bool typeOnly = false)
    {
        var full = type.StartsWith(Constants.GlobalNamespace) ? type : $"{Constants.GlobalNamespace}{type}";
        var converter = typeOnly ? null : $"{Constants.GlobalNamespace}{Constants.ConverterTypeName}<{full}>";
        this.aliases.Add(alias, (full, converter));
    }

    public void Add(ITypeSymbol symbol, string alias, bool typeOnly = false)
    {
        Add(Symbols.GetSymbolFullName(symbol), alias, typeOnly);
    }

    public void AppendAliases(StringBuilder builder)
    {
        foreach (var i in this.aliases)
        {
            var alias = i.Key;
            var (type, converter) = i.Value;
            _ = builder.AppendLine($"using _T{alias} = {type};");
            if (converter is null)
                continue;
            _ = builder.AppendLine($"using _C{alias} = {converter};");
        }
    }
}
