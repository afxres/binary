namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

public interface IGeneratorContext
{
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CommonModule.RequiresDynamicCodeMessage)]
#endif
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    IConverter GetConverter(Type type);
}
