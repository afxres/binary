using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Components
{
    internal readonly struct GenericConverterCreatorContext
    {
        private readonly IReadOnlyDictionary<Type, Type> dictionary;

        public GenericConverterCreatorContext(IReadOnlyDictionary<Type, Type> dictionary)
        {
            Debug.Assert(dictionary.Any());
            Debug.Assert(dictionary.All(x => x.Key.GetGenericArguments().Length == x.Value.GetGenericArguments().Length));
            this.dictionary = dictionary;
        }

        public Converter GetConverter(IGeneratorContext context, Type type, Func<IReadOnlyCollection<Converter>, IEnumerable<object>> func)
        {
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var typeArguments = type.GetGenericArguments();
            var converterType = definition.MakeGenericType(typeArguments);
            var constructor = converterType.GetConstructors().Single();
            var converterTypes = constructor.GetParameters()
                .Select(x => x.ParameterType)
                .TakeWhile(x => x.IsSubclassOf(typeof(Converter)))
                .Select(x => x.GetGenericArguments().Single())
                .ToArray();
            var converters = converterTypes.Select(context.GetConverter).ToList();
            var converterArguments = func.Invoke(converters).ToArray();
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
