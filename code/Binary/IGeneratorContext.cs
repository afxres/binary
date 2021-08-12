namespace Mikodev.Binary;

using System;

public interface IGeneratorContext
{
    IConverter GetConverter(Type type);
}
