using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Components
{
    internal abstract class PatternConverterCreator : IConverterCreator
    {
        private readonly PatternConverterCreatorContext context;

        public PatternConverterCreator(IEnumerable<Type> interfaces, Type assignable, Type definition) => context = new PatternConverterCreatorContext(interfaces, assignable, definition);

        public Converter GetConverter(IGeneratorContext context, Type type) => this.context.GetConverter(context, type);
    }
}
