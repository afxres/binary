using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpSetConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(FSharpSet<>)))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var converterType = typeof(FSharpSetConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, context.GetConverter(itemType));
            return (Converter)converter;
        }
    }
}
