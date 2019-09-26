using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverterCreator : IConverterCreator
    {
        private static readonly GenericTypeMatcher matcher = new GenericTypeMatcher(new Dictionary<Type, Type>
        {
            [typeof(Tuple<>)] = typeof(TupleConverter<>),
            [typeof(Tuple<,>)] = typeof(TupleConverter<,>),
            [typeof(Tuple<,,>)] = typeof(TupleConverter<,,>),
            [typeof(Tuple<,,,>)] = typeof(TupleConverter<,,,>),
            [typeof(Tuple<,,,,>)] = typeof(TupleConverter<,,,,>),
            [typeof(Tuple<,,,,,>)] = typeof(TupleConverter<,,,,,>),
            [typeof(Tuple<,,,,,,>)] = typeof(TupleConverter<,,,,,,>),
            [typeof(Tuple<,,,,,,,>)] = typeof(TupleConverter<,,,,,,,>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!matcher.Match(type, out var converterDefinition))
                return null;
            var types = type.GetGenericArguments();
            var converters = types.Select(context.GetConverter).Cast<object>().ToArray();
            var converterType = converterDefinition.MakeGenericType(types);
            var converter = Activator.CreateInstance(converterType, converters);
            return (Converter)converter;
        }
    }
}
