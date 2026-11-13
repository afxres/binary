namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

public readonly union SourceResult(SourceResult.Success, SourceResult.Ignored, SourceResult.Diagnostic)
{
    public readonly record struct Success(string ConverterCreatorTypeName, string SourceCode);

    public readonly record struct Ignored;

    public readonly record struct Diagnostic(ImmutableArray<(DiagnosticDescriptor Descriptor, object?[]? MessageArguments)> DiagnosticArguments);
}
