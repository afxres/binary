namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

internal sealed class PriorityQueueConverterCreator : IConverterCreator
{
    public IConverter GetConverter(IGeneratorContext context, Type type)
    {
        return CommonHelper.GetConverter(context, type, typeof(PriorityQueue<,>), typeof(PriorityQueueConverter<,>), null);
    }
}
