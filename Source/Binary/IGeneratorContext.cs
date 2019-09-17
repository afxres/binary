using System;

namespace Mikodev.Binary
{
    public interface IGeneratorContext
    {
        /// <summary>
        /// Get or create converter, throw if not supported
        /// </summary>
        Converter GetConverter(Type type);
    }
}
