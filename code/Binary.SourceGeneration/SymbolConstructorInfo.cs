namespace Mikodev.Binary.SourceGeneration;

using System.Collections.Immutable;
using System.Text;
using System.Threading;

public class SymbolConstructorInfo<T> where T : SymbolMemberInfo
{
    private readonly ImmutableArray<T> members;

    private readonly ImmutableArray<T> parameters;

    public SymbolConstructorInfo(ImmutableArray<T> members, ImmutableArray<T> parameters)
    {
        this.members = members;
        this.parameters = parameters;
    }

    public void Append(StringBuilder builder, string typeName, CancellationToken cancellation)
    {
        var members = this.members;
        var parameters = this.parameters;
        var constructorOnly = parameters.Length == members.Length;
        var tail = constructorOnly ? ");" : ")";
        builder.AppendIndent(3, $"var result = new {typeName}(", tail, parameters, x => $"var{members.IndexOf(x)}");
        if (constructorOnly is false)
        {
            builder.AppendIndent(3, $"{{");
            foreach (var i in members)
            {
                if (parameters.Contains(i))
                    continue;
                builder.AppendIndent(4, $"{i.Name} = var{members.IndexOf(i)},");
                cancellation.ThrowIfCancellationRequested();
            }
            builder.AppendIndent(3, $"}};");
        }
        builder.AppendIndent(3, $"return result;");
    }
}
