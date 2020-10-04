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
            return GetGenericArgument(converter.GetType());
        }

        public static Type GetGenericArgument(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            var node = type;
            while ((node = node.BaseType) is not null)
                if (CommonHelper.TryGetGenericArguments(node, typeof(Converter<>), out var arguments))
                    return arguments.Single();
            throw new ArgumentException($"Can not get generic argument, '{type}' is not a subclass of '{typeof(Converter<>)}'");
        }
    }
}
