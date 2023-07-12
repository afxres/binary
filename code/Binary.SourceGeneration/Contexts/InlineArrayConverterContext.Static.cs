namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Linq;

public sealed partial class InlineArrayConverterContext
{
    private class TypeInfo(int length, ITypeSymbol elementType)
    {
        public int Length { get; } = length;

        public ITypeSymbol ElementType { get; } = elementType;
    }

    private static TypeInfo? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type.IsValueType is false)
            return null;
        const string InlineArrayAttribute = "System.Runtime.CompilerServices.InlineArrayAttribute";
        if (context.GetAttribute(type, InlineArrayAttribute) is not { } attribute)
            return null;
        var length = (int)attribute.ConstructorArguments.Single().Value!;
        var element = type.GetMembers().OfType<IFieldSymbol>().Where(x => x.IsStatic is false).Single().Type;
        return new TypeInfo(length, element);
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new InlineArrayConverterContext(context, tracker, symbol, info).Invoke();
    }
}
