using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.TupleLike
{
    internal sealed class TupleLikeConverterCreator : GenericConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(KeyValuePair<,>)] = typeof(KeyValuePairConverter<,>),
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

        public TupleLikeConverterCreator() : base(dictionary) { }

        public override Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            return base.GetConverter(context, type);
        }

        protected override object[] GetArguments(Converter[] converters)
        {
            var length = Define.GetConverterLength(converters);
            var result = converters.Cast<object>().Concat(new object[] { length });
            return result.ToArray();
        }
    }
}
