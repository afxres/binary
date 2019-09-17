using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(Nullable<>)))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var converterType = typeof(NullableConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, context.GetConverter(itemType));
            return (Converter)converter;
        }
    }
}
