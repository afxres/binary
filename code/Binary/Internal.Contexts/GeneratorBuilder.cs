using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class GeneratorBuilder : IGeneratorBuilder
    {
        private readonly Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

        private readonly LinkedList<IConverterCreator> creators = new LinkedList<IConverterCreator>();

        public IGeneratorBuilder AddConverter(IConverter converter)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));
            var itemType = ConverterHelper.GetGenericArgument(converter);
            if (itemType == typeof(object))
                throw new ArgumentException($"Can not add converter for '{typeof(object)}'");
            converters[itemType] = converter;
            return this;
        }

        public IGeneratorBuilder AddConverterCreator(IConverterCreator creator)
        {
            if (creator is null)
                throw new ArgumentNullException(nameof(creator));
            _ = creators.AddFirst(creator);
            return this;
        }

        public IGenerator Build() => new Generator(converters, creators);

        public override string ToString() => $"{nameof(GeneratorBuilder)}(Converters: {converters.Count}, Creators: {creators.Count})";
    }
}
