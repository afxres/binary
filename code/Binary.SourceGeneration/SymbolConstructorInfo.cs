namespace Mikodev.Binary.SourceGeneration;

using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

public class SymbolConstructorInfo<T> where T : SymbolMemberInfo
{
    private readonly ImmutableArray<T> members;

    private readonly ImmutableArray<int> objectIndexes;

    private readonly ImmutableArray<int> directIndexes;

    public SymbolConstructorInfo(ImmutableArray<T> members, ImmutableArray<int> objectIndexes, ImmutableArray<int> directIndexes)
    {
        this.members = members;
        this.objectIndexes = objectIndexes;
        this.directIndexes = directIndexes;
    }

    public void AppendTo(StringBuilder target, string typeName, CancellationToken cancellation)
    {
        var members = this.members;
        var objectIndexes = this.objectIndexes;
        var directIndexes = this.directIndexes;
        var constructorOnly = directIndexes.Length is 0;
        var tail = constructorOnly ? ");" : ")";
        target.AppendIndent(3, $"var result = new {typeName}(", tail, objectIndexes.Length, x => $"var{objectIndexes[x]}");
        if (constructorOnly is false)
        {
            target.AppendIndent(3, $"{{");
            foreach (var i in directIndexes)
            {
                var member = members[i];
                target.AppendIndent(4, $"{member.NameInSourceCode} = var{i},");
                cancellation.ThrowIfCancellationRequested();
            }
            target.AppendIndent(3, $"}};");
        }
        target.AppendIndent(3, $"return result;");
    }
}
