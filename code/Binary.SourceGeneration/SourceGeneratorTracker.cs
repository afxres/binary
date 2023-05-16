namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

public class SourceGeneratorTracker
{
    private readonly Queue<ITypeSymbol> referenced;

    public SourceGeneratorTracker(Queue<ITypeSymbol> referenced)
    {
        this.referenced = referenced;
    }

    public void AddType(ITypeSymbol symbol)
    {
        this.referenced.Enqueue(symbol);
    }
}
