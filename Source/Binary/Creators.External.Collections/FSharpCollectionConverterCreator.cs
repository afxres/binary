using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.External.Collections
{
    internal sealed class FSharpCollectionConverterCreator : GenericConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(FSharpList<>)] = typeof(FSharpListConverter<>),
            [typeof(FSharpSet<>)] = typeof(FSharpSetConverter<>),
            [typeof(FSharpMap<,>)] = typeof(FSharpMapConverter<,>),
        };

        public FSharpCollectionConverterCreator() : base(dictionary) { }
    }
}
