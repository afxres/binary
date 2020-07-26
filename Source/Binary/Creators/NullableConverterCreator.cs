using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (!CommonHelper.TryGetGenericArguments(type, typeof(Nullable<>), out var arguments))
                return null;
            var itemType = arguments.Single();
            var itemConverter = context.GetConverter(itemType);
            var converterArguments = new object[] { itemConverter };
            var converterType = typeof(NullableConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }
    }
}
