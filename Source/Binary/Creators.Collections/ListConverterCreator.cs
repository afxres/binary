using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ListConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(List<>)))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var adapter = Adapter.Create(context.GetConverter(itemType));
            var converter = Activator.CreateInstance(typeof(ListConverter<>).MakeGenericType(itemType), adapter);
            return (Converter)converter;
        }
    }
}
