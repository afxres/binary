using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpListConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(FSharpList<>)))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var converterType = typeof(FSharpListConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, context.GetConverter(itemType));
            return (Converter)converter;
        }
    }
}
