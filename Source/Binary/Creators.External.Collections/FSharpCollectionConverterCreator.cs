using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.External.Collections
{
    internal sealed class FSharpCollectionConverterCreator : IConverterCreator
    {
        private static readonly GenericConverterCreator creator = new GenericConverterCreator(new Dictionary<Type, Type>
        {
            [typeof(FSharpList<>)] = typeof(FSharpListConverter<>),
            [typeof(FSharpSet<>)] = typeof(FSharpSetConverter<>),
            [typeof(FSharpMap<,>)] = typeof(FSharpMapConverter<,>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
