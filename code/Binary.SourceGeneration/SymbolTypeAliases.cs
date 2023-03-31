namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SymbolTypeAliases
{
    private class Record
    {
        public string Suffix { get; }

        public string? FullName { get; }

        public string? ConverterFullName { get; }

        public Record(string fullName, string suffix, SymbolTypeAliasesFlags flags)
        {
            var globalName = fullName.StartsWith(Constants.GlobalNamespacePrefix) ? fullName : $"{Constants.GlobalNamespacePrefix}{fullName}";
            Suffix = suffix;
            FullName = (flags & SymbolTypeAliasesFlags.Type) is 0 ? null : globalName;
            ConverterFullName = (flags & SymbolTypeAliasesFlags.ConverterType) is 0 ? null : $"{Constants.GlobalNamespacePrefix}{Constants.ConverterTypeName}<{globalName}>";
        }
    }

    private readonly Dictionary<string, Record> aliases = new Dictionary<string, Record>();

    public void Add(string type, string suffix, SymbolTypeAliasesFlags flags = SymbolTypeAliasesFlags.All)
    {
        this.aliases.Add(suffix, new Record(type, suffix, flags));
    }

    public void Add(ITypeSymbol symbol, string suffix, SymbolTypeAliasesFlags flags = SymbolTypeAliasesFlags.All)
    {
        this.aliases.Add(suffix, new Record(Symbols.GetSymbolFullName(symbol), suffix, flags));
    }

    public void AppendAliases(StringBuilder builder)
    {
        foreach (var record in this.aliases.Values.OrderBy(x => x.Suffix))
        {
            if (record.FullName is { } type)
                _ = builder.AppendLine($"using _T{record.Suffix} = {type};");
            if (record.ConverterFullName is { } converter)
                _ = builder.AppendLine($"using _C{record.Suffix} = {converter};");
            continue;
        }
    }
}
