namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

public class SymbolConstructorInfo<T> where T : SymbolMemberInfo
{
    public ITypeSymbol Symbol { get; }

    public ImmutableArray<T> Members { get; }

    public ImmutableArray<T> ConstructorParameters { get; }

    public SymbolConstructorInfo(ITypeSymbol symbol, ImmutableArray<T> members, ImmutableArray<T> constructorParameters)
    {
        Symbol = symbol;
        Members = members;
        ConstructorParameters = constructorParameters;
    }

    public void AppendCreateInstance(StringBuilder builder, CancellationToken cancellation)
    {
        var constructorOnly = ConstructorParameters.Length == Members.Length;
        var tail = constructorOnly ? ");" : ")";
        var fullName = Symbols.GetSymbolFullName(Symbol);
        builder.AppendIndent(3, $"var result = new {fullName}(", tail, ConstructorParameters, x => $"var{Members.IndexOf(x)}");
        if (constructorOnly is false)
        {
            builder.AppendIndent(3, $"{{");
            foreach (var i in Members)
            {
                if (ConstructorParameters.Contains(i))
                    continue;
                builder.AppendIndent(4, $"{i.Name} = var{Members.IndexOf(i)},");
                cancellation.ThrowIfCancellationRequested();
            }
            builder.AppendIndent(3, $"}};");
        }
        builder.AppendIndent(3, $"return result;");
    }
}
