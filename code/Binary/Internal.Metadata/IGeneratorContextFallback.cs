namespace Mikodev.Binary.Internal.Metadata;

using System;

internal interface IGeneratorContextFallback
{
    IConverter GetConverter(Type type, IGeneratorContext context);
}
