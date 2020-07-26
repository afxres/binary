using System;

namespace Mikodev.Binary
{
    public interface IGeneratorContext
    {
        IConverter GetConverter(Type type);
    }
}
