using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleLikeConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(Tuple<>)] = typeof(TupleConverter<>),
            [typeof(Tuple<,>)] = typeof(TupleConverter<,>),
            [typeof(Tuple<,,>)] = typeof(TupleConverter<,,>),
            [typeof(Tuple<,,,>)] = typeof(TupleConverter<,,,>),
            [typeof(Tuple<,,,,>)] = typeof(TupleConverter<,,,,>),
            [typeof(Tuple<,,,,,>)] = typeof(TupleConverter<,,,,,>),
            [typeof(Tuple<,,,,,,>)] = typeof(TupleConverter<,,,,,,>),
            [typeof(Tuple<,,,,,,,>)] = typeof(TupleConverter<,,,,,,,>),
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
            static int GetConverterLength(Type type, IReadOnlyCollection<Converter> values)
            {
                Debug.Assert(values.Any() && values.All(x => x != null && x.Length >= 0));
                var length = values.All(x => x.Length > 0) ? values.Sum(x => (long)x.Length) : 0;
                if (length < 0 || length > int.MaxValue)
                    throw new ArgumentException($"Converter length overflow, type: {type}");
                return (int)length;
            }

            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var typeArguments = type.GetGenericArguments();
            var converterType = definition.MakeGenericType(typeArguments);
            var converters = typeArguments.Select(context.GetConverter).ToList();
            var converterLength = GetConverterLength(type, converters);
            var converterArguments = new List<object>(converters) { converterLength }.ToArray();
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
