namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Text;

public partial class EnumConverterContext : SymbolConverterContext
{
    public EnumConverterContext(SourceGeneratorContext context, ITypeSymbol symbol) : base(context, symbol) { }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.GetEnumConverter<_TSelf>();");
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
