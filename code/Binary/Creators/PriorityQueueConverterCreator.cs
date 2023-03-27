namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
internal sealed class PriorityQueueConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        return CommonModule.GetConverter(context, type, typeof(PriorityQueue<,>), typeof(PriorityQueueConverter<,>));
    }
}
