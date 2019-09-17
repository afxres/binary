using System;

namespace Mikodev.Binary
{
    public interface IGenerator
    {
        Converter GetConverter(Type type);
    }
}
