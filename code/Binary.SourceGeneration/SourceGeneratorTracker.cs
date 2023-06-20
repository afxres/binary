namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

public class SourceGeneratorTracker(Queue<ITypeSymbol> referenced)
{
    public void AddType(ITypeSymbol symbol)
    {
        referenced.Enqueue(symbol);
    }
}
