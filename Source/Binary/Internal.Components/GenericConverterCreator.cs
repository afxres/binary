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

        protected virtual object[] GetArguments(Converter[] converters)
        {
            return converters.Cast<object>().ToArray();
        }

        public virtual Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).ToArray();
            var converterType = definition.MakeGenericType(types);
            var arguments = GetArguments(converters);
            var converter = Activator.CreateInstance(converterType, arguments);
            return (Converter)converter;
        }
    }
}
