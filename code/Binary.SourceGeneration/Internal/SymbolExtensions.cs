namespace Mikodev.Binary.SourceGeneration.Internal;

using Microsoft.CodeAnalysis;

public static class SymbolExtensions
{
    public static bool TryGetConstructorArgument<T>(this AttributeData attribute, out T result)
    {
        result = default!;
        var arguments = attribute.ConstructorArguments;
        if (arguments.Length is not 1)
            return false;
        if (arguments[0].Value is not T actual)
            return false;
        result = actual;
        return true;
    }

    public static Diagnostic With(this DiagnosticDescriptor descriptor, ISymbol symbol, object?[]? parameters = null)
    {
        return Diagnostic.Create(descriptor, Symbols.GetLocation(symbol), parameters);
    }

    public static Diagnostic With(this DiagnosticDescriptor descriptor, AttributeData? attribute, object?[]? parameters = null)
    {
        return Diagnostic.Create(descriptor, Symbols.GetLocation(attribute), parameters);
    }
}
