namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Linq;

public sealed partial class InlineArrayConverterContext
{
    private class TypeInfo(int length, ITypeSymbol elementType)
    {
        public int Length { get; } = length;

        public ITypeSymbol ElementType { get; } = elementType;
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (symbol.IsValueType is false)
            return null;
        const string InlineArrayAttribute = "System.Runtime.CompilerServices.InlineArrayAttribute";
        if (context.GetAttribute(symbol, InlineArrayAttribute) is not { } attribute)
            return null;
        var fields = symbol.GetMembers().OfType<IFieldSymbol>().Where(x => x.IsStatic is false).ToList();
        if (fields.Count is not 1 || attribute.TryGetConstructorArgument<int>(out var length) is false)
            return new SourceResult(SourceStatus.Skip);
        return new InlineArrayConverterContext(context, tracker, symbol, new TypeInfo(length, fields[0].Type)).Invoke();
    }
}
