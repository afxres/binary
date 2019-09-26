using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal
{
    internal readonly struct SimpleConverterCreator
    {
        private readonly IReadOnlyDictionary<Type, Type> dictionary;

        public SimpleConverterCreator(IReadOnlyDictionary<Type, Type> dictionary)
        {
            Debug.Assert(dictionary.Any());
            Debug.Assert(dictionary.All(x => x.Key.GetGenericArguments().Length == x.Value.GetGenericArguments().Length));
            this.dictionary = dictionary;
        }

        public Converter GetConverter(IGeneratorContext context, Type type, Func<Converter[], object[]> func = null)
        {
            Debug.Assert(type != null);
            Debug.Assert(context != null);
            Debug.Assert(dictionary != null);
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            func ??= (x => x.Cast<object>().ToArray());
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).ToArray();
            var converterType = definition.MakeGenericType(types);
            var arguments = func.Invoke(converters);
            var converter = Activator.CreateInstance(converterType, arguments);
            return (Converter)converter;
        }
    }
}
