using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class LinkedListConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (CommonHelper.TryGetGenericArguments(type, typeof(LinkedList<>), out var arguments) is false)
                return null;
            var itemType = arguments.Single();
            var itemConverter = context.GetConverter(itemType);
            var converterArguments = new object[] { itemConverter };
            var converterType = typeof(LinkedListConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }
    }
}
