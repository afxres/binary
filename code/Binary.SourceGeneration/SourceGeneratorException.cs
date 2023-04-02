namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Runtime.Serialization;

[Serializable]
public class SourceGeneratorException : Exception
{
    public Diagnostic? Diagnostic { get; set; }

    public SourceGeneratorException() { }

    public SourceGeneratorException(DiagnosticDescriptor descriptor, Location? location, object?[]? arguments) => Diagnostic = Diagnostic.Create(descriptor, location, arguments);

    protected SourceGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
