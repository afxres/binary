namespace Mikodev.Binary;

using System;

public interface IConverterCreator
{
    IConverter GetConverter(IGeneratorContext context, Type type);
}
