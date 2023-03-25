namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public partial class GenericConverterContext : SymbolConverterContext
{
    private readonly string name;

    private readonly ImmutableArray<ITypeSymbol> elements;

    private GenericConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, string name, ImmutableArray<ITypeSymbol> elements) : base(context, symbol)
    {
        for (var i = 0; i < elements.Length; i++)
            TypeAliases.Add(elements[i], i.ToString());
        this.name = name;
        this.elements = elements;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var name = this.name;
        var elements = this.elements;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", $"_C{i}", $"_T{i}");
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.Get{name}Converter(", ");", elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    private void Invoke()
    {
        var builder = new StringBuilder();
        AppendFileHead(builder);

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);

        AppendFileTail(builder);
        Finish(builder);
    }
}
