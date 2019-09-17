using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IListConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetInterfaceArguments(typeof(IEnumerable<>), out var arguments))
                return null;
            var itemType = arguments.Single();
            if (!type.IsAssignableFrom(itemType.MakeArrayType()) || !type.IsAssignableFrom(typeof(List<>).MakeGenericType(itemType)))
                return null;
            var converter = Activator.CreateInstance(typeof(IListConverter<,>).MakeGenericType(type, itemType), context.GetConverter(itemType));
            return (Converter)converter;
        }
    }
}
