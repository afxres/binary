using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(ValueTuple<>)] = typeof(ValueTupleConverter<>),
            [typeof(ValueTuple<,>)] = typeof(ValueTupleConverter<,>),
            [typeof(ValueTuple<,,>)] = typeof(ValueTupleConverter<,,>),
            [typeof(ValueTuple<,,,>)] = typeof(ValueTupleConverter<,,,>),
            [typeof(ValueTuple<,,,,>)] = typeof(ValueTupleConverter<,,,,>),
            [typeof(ValueTuple<,,,,,>)] = typeof(ValueTupleConverter<,,,,,>),
            [typeof(ValueTuple<,,,,,,>)] = typeof(ValueTupleConverter<,,,,,,>),
            [typeof(ValueTuple<,,,,,,,>)] = typeof(ValueTupleConverter<,,,,,,,>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var converterDefinition))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = converterDefinition.MakeGenericType(types);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
