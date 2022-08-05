namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

public interface IConverterCreator
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    IConverter? GetConverter(IGeneratorContext context, Type type);
}
