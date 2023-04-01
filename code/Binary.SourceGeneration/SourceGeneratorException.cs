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

    public SourceGeneratorException(string message) : base(message) { }

    public SourceGeneratorException(string message, Exception inner) : base(message, inner) { }

    protected SourceGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
