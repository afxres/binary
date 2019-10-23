using System;
using System.Collections.Generic;

namespace Mikodev.Binary
{
    public sealed class GeneratorBuilder : IGeneratorBuilder
    {
        private readonly Dictionary<Type, Converter> converters = new Dictionary<Type, Converter>();

        private readonly LinkedList<IConverterCreator> creators = new LinkedList<IConverterCreator>();

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
            _ = creators.AddFirst(creator);
            return this;
        }

        public IGenerator Build() => new Internal.Contexts.Generator(converters.Values, creators);

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(GeneratorBuilder)}(Converters: {converters.Count}, Creators: {creators.Count})";
    }
}
