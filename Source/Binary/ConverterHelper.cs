using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary
{
    public static class ConverterHelper
    {
        public static Type GetGenericArgument(IConverter converter)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));
            for (var i = converter.GetType(); i != null; i = i.BaseType)
                if (CommonHelper.TryGetGenericArguments(i, typeof(Converter<>), out var arguments))
                    return arguments.Single();
            throw new ArgumentException($"Invalid converter type, '{converter.GetType()}' is not a subclass of '{typeof(Converter<>)}'");
        }
    }
}
