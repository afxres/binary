using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!CommonHelper.TryGetGenericArguments(type, typeof(KeyValuePair<,>), out var arguments))
                return null;
            var itemConverters = arguments.Select(context.GetConverter).ToList();
            var itemLength = ContextMethods.GetItemLength(itemConverters);
            var converterArguments = new object[] { itemConverters[0], itemConverters[1], itemLength };
            var converterType = typeof(KeyValuePairConverter<,>).MakeGenericType(arguments);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
