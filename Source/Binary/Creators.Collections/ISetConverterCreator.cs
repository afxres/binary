using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverterCreator : IConverterCreator
    {
        private static readonly PatternConverterCreator creator = new PatternConverterCreator(
            new[] { typeof(ISet<>) },
            typeof(HashSet<>),
            typeof(ISetConverter<,>));

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
