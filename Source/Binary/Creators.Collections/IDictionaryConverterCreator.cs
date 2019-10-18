using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverterCreator : IConverterCreator
    {
        private static readonly PatternConverterCreator creator = new PatternConverterCreator(
            new[] { typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>) },
            typeof(Dictionary<,>),
            typeof(IDictionaryConverter<,,>));

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
