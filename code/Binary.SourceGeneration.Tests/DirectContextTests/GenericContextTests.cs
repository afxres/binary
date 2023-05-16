﻿namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class GenericContextTests
{
    [Fact(DisplayName = "Invalid Symbol Test")]
    public void InvalidSymbolTest()
    {
        var compilation = CompilationModule.CreateCompilation(string.Empty);
        var int32Symbol = compilation.GetSpecialType(SpecialType.System_Int32);
        var int32PointerSymbol = compilation.CreatePointerTypeSymbol(int32Symbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(new Queue<ITypeSymbol>());
        var a = GenericConverterContext.Invoke(context, tracker, int32Symbol);
        var b = GenericConverterContext.Invoke(context, tracker, int32PointerSymbol);
        Assert.Null(a);
        Assert.Null(b);
    }
}
