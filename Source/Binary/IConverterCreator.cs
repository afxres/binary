using System;

namespace Mikodev.Binary
{
    public interface IConverterCreator
    {
        /// <summary>
        /// Try create converter, return null if not supported
        /// </summary>
        Converter GetConverter(IGeneratorContext context, Type type);
    }
}
