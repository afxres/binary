namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    public static string GetSymbolDiagnosticDisplay(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDiagnosticDisplayFormat);
    }

    public static string GetNameInSourceCode(string name)
    {
        return IsKeyword(name) ? $"@{name}" : name;
    }

    public static void GetNameInSourceCode(StringBuilder builder, string name)
    {
        if (IsKeyword(name))
            _ = builder.Append('@');
        _ = builder.Append(name);
    }

    public static string GetNamespaceInSourceCode(INamespaceSymbol @namespace)
    {
        var builder = new StringBuilder();
        GetNamespaceInSourceCode(builder, @namespace);
        return builder.ToString();
    }

    public static void GetNamespaceInSourceCode(StringBuilder builder, INamespaceSymbol @namespace)
    {
        var source = @namespace.ToDisplayParts();
        foreach (var i in source)
        {
            if (i.Symbol is { } symbol)
                GetNameInSourceCode(builder, symbol.Name);
            else
                _ = builder.Append('.');
        }
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

        static void InvokeNamespaceSymbol(StringBuilder target, INamespaceSymbol @namespace)
        {
            const string Global = "global::";
            _ = target.Append(Global);
            GetNamespaceInSourceCode(target, @namespace);
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
            var containing = symbol.ContainingType;
            var @namespace = symbol.ContainingNamespace;
            if (containing is not null)
                Invoke(target, containing);
            else
                InvokeNamespaceSymbol(target, @namespace);

            if (containing is not null || @namespace.IsGlobalNamespace is false)
                _ = target.Append('.');
            GetNameInSourceCode(target, symbol.Name);
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

        static void InvokeNamespaceSymbol(StringBuilder target, INamespaceSymbol @namespace)
        {
            if (@namespace.IsGlobalNamespace is false)
            {
                var source = @namespace.ToDisplayParts();
                foreach (var i in source)
                {
                    if (i.Symbol is { } symbol)
                        _ = target.Append(symbol.Name);
                    else
                        _ = target.Append('_');
                }
                return;
            }
            _ = target.Append("g_");
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
            else
                InvokeNamespaceSymbol(target, @namespace);

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
