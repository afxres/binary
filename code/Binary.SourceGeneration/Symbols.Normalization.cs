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

    public static string GetSymbolFullName(ITypeSymbol symbol)
    {
        static void Invoke(StringBuilder target, ITypeSymbol symbol)
        {
            if (symbol is IArrayTypeSymbol array)
                InvokeArrayTypeSymbol(target, array);
            else
                InvokeNamedTypeSymbol(target, (INamedTypeSymbol)symbol);
        }

        static void InvokeArrayTypeSymbol(StringBuilder target, IArrayTypeSymbol symbol)
        {
            Invoke(target, symbol.ElementType);
            _ = target.Append('[');
            for (var i = 0; i < symbol.Rank - 1; i++)
                _ = target.Append(',');
            _ = target.Append(']');
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            _ = target.Append(Constants.GlobalNamespace);
            var @namespace = symbol.ContainingNamespace;
            if (@namespace.IsGlobalNamespace is false)
                _ = target.Append(@namespace.ToDisplayString() + ".");
            _ = target.Append(symbol.Name);
            if (symbol.IsGenericType is false)
                return;
            _ = target.Append("<");
            var arguments = symbol.TypeArguments;
            for (var i = 0; i < arguments.Length; i++)
            {
                Invoke(target, arguments[i]);
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append(", ");
            }
            _ = target.Append(">");
        }

        var target = new StringBuilder();
        Invoke(target, symbol);
        var result = target.ToString();
        return result;
    }

    public static string GetOutputFullName(ITypeSymbol symbol)
    {
        static void Invoke(StringBuilder target, ITypeSymbol symbol)
        {
            if (symbol is IArrayTypeSymbol array)
                InvokeArrayTypeSymbol(target, array);
            else
                InvokeNamedTypeSymbol(target, (INamedTypeSymbol)symbol);
        }

        static void InvokeArrayTypeSymbol(StringBuilder target, IArrayTypeSymbol symbol)
        {
            _ = target.Append("Array");
            if (symbol.Rank is not 1)
                _ = target.Append($"{symbol.Rank}D");
            _ = target.Append('_');
            Invoke(target, symbol.ElementType);
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            var @namespace = symbol.ContainingNamespace;
            if (@namespace.IsGlobalNamespace is false)
                _ = target.Append(@namespace.ToDisplayString() + ".");
            _ = target.Replace('.', '_');
            _ = target.Append(symbol.Name);
            if (symbol is INamedTypeSymbol type && type.IsGenericType)
            {
                _ = target.Append("_l_");
                var arguments = type.TypeArguments;
                for (var i = 0; i < arguments.Length; i++)
                {
                    Invoke(target, arguments[i]);
                    if (i == arguments.Length - 1)
                        break;
                    _ = target.Append("_c_");
                }
                _ = target.Append("_r");
            }
        }

        var target = new StringBuilder();
        Invoke(target, symbol);
        var result = target.ToString();
        return result;
    }
}
