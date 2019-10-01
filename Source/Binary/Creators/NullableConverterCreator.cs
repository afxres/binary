using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : IConverterCreator
    {
        private static readonly GenericConverterCreator creator = new GenericConverterCreator(new Dictionary<Type, Type>
        {
            [typeof(Nullable<>)] = typeof(NullableConverter<>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
