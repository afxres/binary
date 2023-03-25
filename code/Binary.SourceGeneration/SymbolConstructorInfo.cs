namespace Mikodev.Binary.SourceGeneration;

using System.Collections.Immutable;
using System.Text;
using System.Threading;

public class SymbolConstructorInfo<T> where T : SymbolMemberInfo
{
    public ImmutableArray<T> Members { get; }

    public ImmutableArray<T> ConstructorParameters { get; }

    public SymbolConstructorInfo(ImmutableArray<T> members, ImmutableArray<T> constructorParameters)
    {
        Members = members;
        ConstructorParameters = constructorParameters;
    }

    public void AppendCreateInstance(StringBuilder builder, CancellationToken cancellation)
    {
        var constructorOnly = ConstructorParameters.Length == Members.Length;
        var tail = constructorOnly ? ");" : ")";
        builder.AppendIndent(3, $"var result = new _TSelf(", tail, ConstructorParameters, x => $"var{Members.IndexOf(x)}");
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
