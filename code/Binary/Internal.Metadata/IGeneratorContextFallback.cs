namespace Mikodev.Binary.Internal.Metadata;

using System;

internal interface IGeneratorContextFallback
{
    IConverter GetConverter(IGeneratorContext context, Type type);
}
