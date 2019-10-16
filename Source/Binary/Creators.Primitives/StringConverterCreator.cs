using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class StringConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type != typeof(string))
                return null;
            return new StringConverter();
        }
    }
}
