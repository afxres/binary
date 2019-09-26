using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Tuples
{
    internal sealed class TupleConverterCreator : IConverterCreator
    {
        private static readonly SimpleConverterCreator creator = new SimpleConverterCreator(new Dictionary<Type, Type>
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
            return creator.GetConverter(context, type);
        }
    }
}
