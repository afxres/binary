using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(KeyValuePair<,>)))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(types);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
