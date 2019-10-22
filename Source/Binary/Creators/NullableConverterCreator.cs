using Mikodev.Binary.Internal.Extensions;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetGenericArguments(typeof(Nullable<>), out var arguments))
                return null;
            var itemType = arguments.Single();
            var itemConverter = context.GetConverter(itemType);
            var converterType = typeof(NullableConverter<>).MakeGenericType(itemType);
            var converterArguments = new object[] { itemConverter };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
