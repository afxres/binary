using System;
using System.Collections.Generic;

namespace Mikodev.Binary
{
    public sealed class GeneratorBuilder : IGeneratorBuilder
    {
        private readonly Dictionary<Type, Converter> converters = new Dictionary<Type, Converter>();

        private readonly List<IConverterCreator> creators = new List<IConverterCreator>();

        public IGeneratorBuilder AddConverter(Converter converter)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));
            var itemType = converter.ItemType;
            if (itemType == typeof(object))
                throw new ArgumentException($"Can not add converter for '{typeof(object)}'");
            converters[itemType] = converter;
            return this;
        }

        public IGeneratorBuilder AddConverterCreator(IConverterCreator creator)
        {
            if (creator is null)
                throw new ArgumentNullException(nameof(creator));
            creators.Add(creator);
            return this;
        }

        public IGenerator Build() => new Internal.Contexts.Generator(converters.Values, creators);
    }
}
