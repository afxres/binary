namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;

public static partial class Symbols
{
    public static SymbolDisplayFormat SymbolDiagnosticDisplayFormat { get; } = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.ExpandNullable);

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

    public static string GetSymbolDiagnosticDisplay(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDiagnosticDisplayFormat);
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
            _ = target.Append(',', symbol.Rank - 1);
            _ = target.Append(']');
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            const string Global = "global::";
            var containing = symbol.ContainingType;
            var @namespace = symbol.ContainingNamespace;
            if (containing is not null)
                Invoke(target, containing);
            else if (@namespace.IsGlobalNamespace)
                _ = target.Append(Global);
            else
                _ = target.Append(Global + @namespace.ToDisplayString());

            if (containing is not null || @namespace.IsGlobalNamespace is false)
                _ = target.Append('.');
            _ = target.Append(symbol.Name);
            var arguments = symbol.TypeArguments;
            if (arguments.Length is 0)
                return;

            _ = target.Append("<");
            for (var i = 0; i < arguments.Length; i++)
            {
                Invoke(target, arguments[i]);
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append(", ");
            }
            _ = target.Append(">");
        }

        var builder = new StringBuilder();
        Invoke(builder, symbol);
        return builder.ToString();
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
            _ = target.Append("_l_");
            Invoke(target, symbol.ElementType);
            _ = target.Append("_r");
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            var containing = symbol.ContainingType;
            var @namespace = symbol.ContainingNamespace;
            if (containing is not null)
                Invoke(target, containing);
            else if (@namespace.IsGlobalNamespace)
                _ = target.Append("g_");
            else
                _ = target.Append(@namespace.ToDisplayString().Replace('.', '_'));

            if (containing is not null || @namespace.IsGlobalNamespace is false)
                _ = target.Append('_');
            _ = target.Append(symbol.Name);
            var arguments = symbol.TypeArguments;
            if (arguments.Length is 0)
                return;

            _ = target.Append("_l_");
            for (var i = 0; i < arguments.Length; i++)
            {
                Invoke(target, arguments[i]);
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append("_c_");
            }
            _ = target.Append("_r");
        }

        var builder = new StringBuilder();
        Invoke(builder, symbol);
        return builder.ToString();
    }
}
