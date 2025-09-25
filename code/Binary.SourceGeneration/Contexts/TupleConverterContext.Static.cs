namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

public sealed partial class TupleConverterContext
{
    private static bool IsSystemValueTuple(ITypeSymbol symbol)
    {
        return symbol.IsValueType && symbol.IsTupleType;
    }

    private static void GetMembers(ITypeSymbol symbol, string prefix, ImmutableArray<SymbolTupleMemberInfo>.Builder result)
    {
        Debug.Assert(IsSystemValueTuple(symbol));
        var realFields = symbol.GetMembers()
            .Select(x => (x as IFieldSymbol)?.CorrespondingTupleField)
            .OfType<IFieldSymbol>()
            .OrderBy(x => x.Name, StringComparer.InvariantCulture)
            .ToImmutableArray();
        foreach (var i in realFields)
        {
            var path = $"{prefix}.{i.Name}";
            var type = i.Type;
            if (IsSystemValueTuple(type))
                GetMembers(type, path, result);
            else
                result.Add(new SymbolTupleMemberInfo(i, path));
        }
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (IsSystemValueTuple(symbol) is false)
            return null;
        var builder = ImmutableArray.CreateBuilder<SymbolTupleMemberInfo>();
        GetMembers(symbol, string.Empty, builder);
        var members = builder.DrainToImmutable();
        if (members.Length is 0)
            return new SourceResult(SourceStatus.NoAvailableMember);
        return new TupleConverterContext(context, tracker, symbol, members).Invoke();
    }
}
