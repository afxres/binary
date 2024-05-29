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

    public static string GetSymbolDiagnosticDisplayString(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDiagnosticDisplayFormat);
    }

    public static string GetNameInSourceCode(string name)
    {
        return IsKeyword(name) ? $"@{name}" : name;
    }

    public static void GetNameInSourceCode(StringBuilder target, string name)
    {
        if (IsKeyword(name))
            _ = target.Append('@');
        _ = target.Append(name);
    }

    public static string GetNamespaceInSourceCode(INamespaceSymbol @namespace)
    {
        var target = new StringBuilder();
        GetNamespaceInSourceCode(target, @namespace);
        return target.ToString();
    }

    public static void GetNamespaceInSourceCode(StringBuilder target, INamespaceSymbol @namespace)
    {
        var source = @namespace.ToDisplayParts();
        foreach (var i in source)
        {
            if (i.Symbol is { } symbol)
                GetNameInSourceCode(target, symbol.Name);
            else
                _ = target.Append('.');
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

            _ = target.Append('<');
            for (var i = 0; i < arguments.Length; i++)
            {
                Invoke(target, arguments[i]);
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append(", ");
            }
            _ = target.Append('>');
        }

        var target = new StringBuilder();
        Invoke(target, symbol);
        return target.ToString();
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
            _ = target.Append('g');
            if (@namespace.IsGlobalNamespace)
                return;
            foreach (var i in @namespace.ToDisplayParts())
                if (i.Symbol is { } symbol)
                    _ = target.AppendFormat("_{0}", symbol.Name);
        }

        static void InvokeArrayTypeSymbol(StringBuilder target, IArrayTypeSymbol symbol)
        {
            _ = target.Append("a_");
            _ = target.Append(symbol.Rank);
            _ = target.Append("_p_");
            Invoke(target, symbol.ElementType);
            _ = target.Append("_q");
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            var containing = symbol.ContainingType;
            var @namespace = symbol.ContainingNamespace;
            if (containing is not null)
                Invoke(target, containing);
            else
                InvokeNamespaceSymbol(target, @namespace);

            var arguments = symbol.TypeArguments;
            _ = target.Append('_');
            _ = target.Append(arguments.Length);
            _ = target.Append('_');
            _ = target.Append(symbol.Name);
            if (arguments.Length is 0)
                return;

            _ = target.Append("_b_");
            for (var i = 0; i < arguments.Length; i++)
            {
                Invoke(target, arguments[i]);
                if (i == arguments.Length - 1)
                    break;
                _ = target.Append('_');
            }
            _ = target.Append("_d");
        }

        var target = new StringBuilder();
        Invoke(target, symbol);
        return target.ToString();
    }
}
