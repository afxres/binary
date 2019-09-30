using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ListConverterCreator : GenericConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(List<>)] = typeof(ListConverter<>),
        };

        public ListConverterCreator() : base(dictionary) { }
    }
}
