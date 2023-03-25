namespace Mikodev.Binary.SourceGeneration;

using System;
using System.Runtime.Serialization;

[Serializable]
public class SourceGeneratorException : Exception
{
    public SourceGeneratorException() { }

    public SourceGeneratorException(string message) : base(message) { }

    public SourceGeneratorException(string message, Exception inner) : base(message, inner) { }

    protected SourceGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
