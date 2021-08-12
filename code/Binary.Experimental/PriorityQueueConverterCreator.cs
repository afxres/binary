namespace Mikodev.Binary.Experimental;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class PriorityQueueConverterCreator : IConverterCreator
{
    public IConverter GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsGenericType is false || type.GetGenericTypeDefinition() != typeof(PriorityQueue<,>))
            return null;
        var arguments = type.GetGenericArguments();
        var converters = arguments.Select(context.GetConverter).ToArray();
        var converterArguments = converters.Cast<object>().ToArray();
        var converterType = typeof(PriorityQueueConverter<,>).MakeGenericType(arguments);
        var converter = Activator.CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }
}
