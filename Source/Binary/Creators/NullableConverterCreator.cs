using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : GenericConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(Nullable<>)] = typeof(NullableConverter<>),
        };

        public NullableConverterCreator() : base(dictionary) { }
    }
}
