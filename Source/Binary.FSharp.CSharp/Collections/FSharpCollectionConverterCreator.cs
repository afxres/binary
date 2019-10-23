using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Collections
{
    internal sealed class FSharpCollectionConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(FSharpList<>)] = typeof(FSharpListConverter<>),
            [typeof(FSharpSet<>)] = typeof(FSharpSetConverter<>),
            [typeof(FSharpMap<,>)] = typeof(FSharpMapConverter<,>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType)
                return null;
            var definition = type.GetGenericTypeDefinition();
            if (!dictionary.TryGetValue(definition, out var result))
                return null;
            var typeArguments = type.GetGenericArguments();
            var converterType = result.MakeGenericType(typeArguments);
            var constructor = converterType.GetConstructors().Single();
            var itemTypes = constructor.GetParameters().Select(x => x.ParameterType).Select(x => x.GetGenericArguments().Single()).ToList();
            var itemConverters = itemTypes.Select(context.GetConverter).ToArray();
            var converterArguments = itemConverters.Cast<object>().ToArray();
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
