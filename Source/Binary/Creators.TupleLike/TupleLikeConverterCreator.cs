using Mikodev.Binary.Creators.TupleLike.Tuples;
using Mikodev.Binary.Creators.TupleLike.ValueTuples;
using Mikodev.Binary.Internal.Components;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.TupleLike
{
    internal sealed class TupleLikeConverterCreator : IConverterCreator
    {
        private static readonly GenericConverterCreator creator = new GenericConverterCreator(new Dictionary<Type, Type>
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
        });

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            return creator.GetConverter(context, type, x => new object[] { ContextMethods.GetConverterLength(type, x) });
        }
    }
}
