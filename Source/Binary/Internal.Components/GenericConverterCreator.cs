using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Components
{
    internal abstract class GenericConverterCreator : IConverterCreator
    {
        private readonly IReadOnlyDictionary<Type, Type> dictionary;

        public GenericConverterCreator(Type key, Type value) : this(new Dictionary<Type, Type> { [key] = value }) { }

        public GenericConverterCreator(IReadOnlyDictionary<Type, Type> dictionary)
        {
            Debug.Assert(dictionary.Any());
            Debug.Assert(dictionary.All(x => x.Key.GetGenericArguments().Length == x.Value.GetGenericArguments().Length));
            this.dictionary = dictionary;
        }

        public virtual Converter GetConverter(IGeneratorContext context, Type type)
        {
            Debug.Assert(type != null);
            Debug.Assert(context != null);
            Debug.Assert(dictionary != null);
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).ToArray();
            var converterType = definition.MakeGenericType(types);
            var arguments = converters.Cast<object>().ToArray();
            var converter = Activator.CreateInstance(converterType, arguments);
            return (Converter)converter;
        }
    }
}
