namespace Mikodev.Binary.SourceGeneration;

public class SourceResult
{
    public SourceStatus Status { get; }

    public string ConverterCreatorTypeName { get; }

    public string SourceCode { get; }

    public SourceResult(SourceStatus status) : this(status, string.Empty, string.Empty) { }

    public SourceResult(string converterCreatorTypeName, string sourceCode) : this(SourceStatus.Ok, converterCreatorTypeName, sourceCode) { }

    public SourceResult(SourceStatus status, string converterCreatorTypeName, string sourceCode)
    {
        Status = status;
        ConverterCreatorTypeName = converterCreatorTypeName;
        SourceCode = sourceCode;
    }
}
