namespace Mikodev.Binary.SourceGeneration;

public class SymbolConverterContent
{
    public string ConverterCreatorTypeName { get; }

    public string Code { get; }

    public SymbolConverterContent(string converterCreatorTypeName, string code)
    {
        ConverterCreatorTypeName = converterCreatorTypeName;
        Code = code;
    }
}
