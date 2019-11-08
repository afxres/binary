using System;

namespace Mikodev.Binary
{
    public interface IGeneratorContext
    {
        Converter GetConverter(Type type);
    }
}
