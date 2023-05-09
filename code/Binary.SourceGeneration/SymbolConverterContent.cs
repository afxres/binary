namespace Mikodev.Binary.SourceGeneration;

public class SymbolConverterContent
{
    public string ConverterCreatorTypeName { get; }

    public string SourceCode { get; }

    public SymbolConverterContent(string creator, string code)
    {
        ConverterCreatorTypeName = creator;
        SourceCode = code;
    }
}
