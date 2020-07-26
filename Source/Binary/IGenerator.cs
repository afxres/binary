using System;

namespace Mikodev.Binary
{
    public interface IGenerator
    {
        IConverter GetConverter(Type type);
    }
}
