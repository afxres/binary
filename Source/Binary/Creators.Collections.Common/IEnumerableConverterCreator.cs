using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class IEnumerableConverterCreator : IConverterCreator
    {
        private static readonly PatternConverterCreator creator = new PatternConverterCreator(
            new[] { typeof(IEnumerable<>) },
            typeof(ArraySegment<>),
            typeof(IEnumerableConverter<,>));

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
