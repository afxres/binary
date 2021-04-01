#if NET5_0_OR_GREATER

using System;
using System.Text;

namespace Mikodev.Binary.Creators
{
    internal sealed class RuneConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (type != typeof(Rune))
                return null;
            return new RuneConverter();
        }
    }
}

#endif
