namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

public interface IGeneratorContext
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    IConverter GetConverter(Type type);
}
