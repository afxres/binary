using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Components
{
    internal abstract class GenericConverterCreator : IConverterCreator
    {
        private readonly GenericConverterCreatorContext creator;

        public GenericConverterCreator(Type key, Type value) : this(new Dictionary<Type, Type> { { key, value } }) { }

        public GenericConverterCreator(IReadOnlyDictionary<Type, Type> dictionary) => creator = new GenericConverterCreatorContext(dictionary);

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
