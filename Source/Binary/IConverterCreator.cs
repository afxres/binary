using System;

namespace Mikodev.Binary
{
    public interface IConverterCreator
    {
        Converter GetConverter(IGeneratorContext context, Type type);
    }
}
