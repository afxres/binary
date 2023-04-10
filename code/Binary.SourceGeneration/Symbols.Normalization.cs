namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;

public static partial class Symbols
{
    public static SymbolDisplayFormat FullDisplayFormat { get; } = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static SymbolDisplayFormat FullDisplayFormatNoNamespace { get; } = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static string ToLiteral(string input)
    {
        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();
    }

    public static Location GetLocation(ISymbol symbol)
    {
        return symbol.Locations.FirstOrDefault() ?? Location.None;
    }

    public static Location GetLocation(AttributeData? attribute)
    {
        var reference = attribute?.ApplicationSyntaxReference;
        if (reference is not null)
            return Location.Create(reference.SyntaxTree, reference.Span);
        return Location.None;
    }

    public static string GetDiagnosticName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(FullDisplayFormatNoNamespace);
    }

    public static string GetSymbolFullName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return typeSymbol.ToDisplayString(FullDisplayFormat);
        var @namespace = symbol.ContainingNamespace.ToDisplayString(FullDisplayFormat);
        var builder = new StringBuilder(@namespace);
        _ = builder.Append(".");
        _ = builder.Append(typeSymbol.Name);
        _ = builder.Append("<");
        var arguments = symbol.TypeArguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            _ = builder.Append(GetSymbolFullName(arguments[i]));
            if (i == arguments.Length - 1)
                break;
            _ = builder.Append(", ");
        }
        _ = builder.Append(">");
        var result = builder.ToString();
        return result;
    }

    public static string GetOutputFullName(ITypeSymbol symbol)
    {
        var prefix = symbol.ContainingNamespace.ToDisplayString(FullDisplayFormat);
        var target = new StringBuilder(prefix);
        _ = target.Replace(Constants.GlobalNamespace, string.Empty);
        _ = target.Replace('.', '_');
        _ = target.Append('_');
        _ = target.Append(symbol.Name);
        if (symbol is INamedTypeSymbol type && type.IsGenericType)
        {
            _ = target.Append("_l_");
            var arguments = type.TypeArguments;
            for (var i = 0; i < arguments.Length; i++)
            {
                _ = target.Append(GetOutputFullName(arguments[i]));
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append("_c_");
            }
            _ = target.Append("_r");
        }
        var result = target.ToString();
        return result;
    }
}
