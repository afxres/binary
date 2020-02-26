using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class Generator : IGenerator
    {
        private readonly ConcurrentDictionary<Type, Converter> converters;

        private readonly IReadOnlyCollection<IConverterCreator> creators;

        public Generator(IEnumerable<Converter> converters, IEnumerable<IConverterCreator> creators)
        {
            Debug.Assert(!converters.Any() || converters.All(x => x is Converter));
            Debug.Assert(!creators.Any() || creators.All(x => x is IConverterCreator));
            var dictionary = converters.ToDictionary(x => x.ItemType);
            Debug.Assert(!dictionary.ContainsKey(typeof(object)));
            this.converters = new ConcurrentDictionary<Type, Converter>(dictionary) { [typeof(object)] = new ObjectConverter(this) };
            this.creators = creators.ToArray();
        }

        public Converter GetConverter(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (converters.TryGetValue(type, out var result))
                return result;
            var context = new GeneratorContext(converters, creators);
            return context.GetConverter(type);
        }

        public override string ToString() => $"{nameof(Generator)}(Converters: {converters.Count}, Creators: {creators.Count})";
    }
}
