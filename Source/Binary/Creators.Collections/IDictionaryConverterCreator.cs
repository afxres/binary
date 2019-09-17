using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetInterfaceArguments(typeof(IDictionary<,>), out var arguments) && !type.TryGetInterfaceArguments(typeof(IReadOnlyDictionary<,>), out arguments))
                return null;
            if (!type.IsAssignableFrom(typeof(Dictionary<,>).MakeGenericType(arguments)))
                return null;
            var converters = arguments.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = typeof(IDictionaryConverter<,,>).MakeGenericType(type, arguments[0], arguments[1]);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
