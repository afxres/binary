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
}
