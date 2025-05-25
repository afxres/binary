namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
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
                InvokeOtherTypeSymbol(target, (INamedTypeSymbol)symbol);
        }

        static void InvokeArrayTypeSymbol(StringBuilder target, IArrayTypeSymbol symbol)
        {
            var prefix = symbol.IsSZArray ? "Array" : $"Array{symbol.Rank}D";
            _ = target.AppendFormat("{0}{1}", prefix.Length, prefix);
            _ = target.Append('I');
            Invoke(target, symbol.ElementType);
            _ = target.Append('E');
        }

        static void InvokeOtherTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            var tokens = new LinkedList<INamedTypeSymbol>();
            for (var i = symbol; i is not null; i = i.ContainingType)
                _ = tokens.AddFirst(i);
            var @namespace = tokens.First.Value.ContainingNamespace;
            var nested = @namespace.IsGlobalNamespace is false || tokens.Count is not 1;
            if (nested)
                _ = target.Append('N');
            foreach (var i in @namespace.ToDisplayParts())
                if (i.Symbol?.Name is { Length: not 0 } name)
                    _ = target.AppendFormat("{0}{1}", name.Length, name);
            foreach (var i in tokens)
                InvokeNamedTypeSymbol(target, i);
            if (nested)
                _ = target.Append('E');
        }

        static void InvokeNamedTypeSymbol(StringBuilder target, INamedTypeSymbol symbol)
        {
            _ = target.AppendFormat("{0}{1}", symbol.Name.Length, symbol.Name);
            var arguments = symbol.TypeArguments;
            if (arguments.Length is 0)
                return;

            _ = target.Append('I');
            for (var i = 0; i < arguments.Length; i++)
                Invoke(target, arguments[i]);
            _ = target.Append('E');
        }

        var target = new StringBuilder("_Z");
        Invoke(target, symbol);
        return target.ToString();
    }
}
