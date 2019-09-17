using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetInterfaceArguments(typeof(ISet<>), out var arguments))
                return null;
            var itemType = arguments.Single();
            if (!type.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(itemType)))
                return null;
            var converter = Activator.CreateInstance(typeof(ISetConverter<,>).MakeGenericType(type, itemType), context.GetConverter(itemType));
            return (Converter)converter;
        }
    }
}
