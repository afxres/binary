using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal;
using System;
using System.Linq;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpMapConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsImplementationOf(typeof(FSharpMap<,>)))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = typeof(FSharpMapConverter<,>).MakeGenericType(types);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
