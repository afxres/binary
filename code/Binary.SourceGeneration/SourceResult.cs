namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

public class SourceResult(SourceStatus status)
{
    public SourceStatus Status { get; } = status;
}

public class SourceResultWithSourceCode(string converterCreatorTypeName, string sourceCode) : SourceResult(SourceStatus.Ok)
{
    public string ConverterCreatorTypeName { get; } = converterCreatorTypeName;

    public string SourceCode { get; } = sourceCode;
}

public class SourceResultWithDiagnostic(ImmutableArray<(DiagnosticDescriptor Descriptor, object?[]? MessageArguments)> diagnosticArguments) : SourceResult(SourceStatus.Error)
{
    public ImmutableArray<(DiagnosticDescriptor Descriptor, object?[]? MessageArguments)> DiagnosticArguments { get; } = diagnosticArguments;
}
