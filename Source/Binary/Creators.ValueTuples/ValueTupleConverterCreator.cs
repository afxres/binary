using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ValueTuples
{
    internal sealed class ValueTupleConverterCreator : IConverterCreator
    {
        private static readonly SimpleConverterCreator creator = new SimpleConverterCreator(new Dictionary<Type, Type>
        {
            [typeof(ValueTuple<>)] = typeof(ValueTupleConverter<>),
            [typeof(ValueTuple<,>)] = typeof(ValueTupleConverter<,>),
            [typeof(ValueTuple<,,>)] = typeof(ValueTupleConverter<,,>),
            [typeof(ValueTuple<,,,>)] = typeof(ValueTupleConverter<,,,>),
            [typeof(ValueTuple<,,,,>)] = typeof(ValueTupleConverter<,,,,>),
            [typeof(ValueTuple<,,,,,>)] = typeof(ValueTupleConverter<,,,,,>),
            [typeof(ValueTuple<,,,,,,>)] = typeof(ValueTupleConverter<,,,,,,>),
            [typeof(ValueTuple<,,,,,,,>)] = typeof(ValueTupleConverter<,,,,,,,>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            return creator.GetConverter(context, type);
        }
    }
}
