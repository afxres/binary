using System;

namespace Mikodev.Binary
{
    public interface IConverterCreator
    {
        IConverter GetConverter(IGeneratorContext context, Type type);
    }
}
