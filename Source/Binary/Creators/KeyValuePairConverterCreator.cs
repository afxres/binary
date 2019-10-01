using Mikodev.Binary.Internal.Components;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(KeyValuePair<,>)] = typeof(KeyValuePairConverter<,>),
        };

        private static readonly GenericConverterCreator creator = new GenericConverterCreator(dictionary);

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type, x => new object[] { ContextMethods.GetConverterLength(type, x) });
    }
}
