namespace Mikodev.Binary;

using System;

public interface IGenerator
{
    IConverter GetConverter(Type type);
}
