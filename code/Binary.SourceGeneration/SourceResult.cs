namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SourceResult
{
    public SourceStatus Status { get; }

    public Diagnostic? Diagnostic { get; }

    public string ConverterCreatorTypeName { get; }

    public string SourceCode { get; }

    public SourceResult(SourceStatus status) : this(status, null, string.Empty, string.Empty) { }

    public SourceResult(Diagnostic diagnostic) : this(SourceStatus.None, diagnostic, string.Empty, string.Empty) { }

    public SourceResult(string converterCreatorTypeName, string sourceCode) : this(SourceStatus.Ok, null, converterCreatorTypeName, sourceCode) { }

    public SourceResult(SourceStatus status, Diagnostic? diagnostic, string converterCreatorTypeName, string sourceCode)
    {
        Status = status;
        Diagnostic = diagnostic;
        ConverterCreatorTypeName = converterCreatorTypeName;
        SourceCode = sourceCode;
    }
}
