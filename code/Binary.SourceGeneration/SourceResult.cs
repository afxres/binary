namespace Mikodev.Binary.SourceGeneration;

public class SourceResult(SourceStatus status, string converterCreatorTypeName, string sourceCode)
{
    public SourceStatus Status { get; } = status;

    public string ConverterCreatorTypeName { get; } = converterCreatorTypeName;

    public string SourceCode { get; } = sourceCode;

    public SourceResult(SourceStatus status) : this(status, string.Empty, string.Empty) { }

    public SourceResult(string converterCreatorTypeName, string sourceCode) : this(SourceStatus.Ok, converterCreatorTypeName, sourceCode) { }
}
