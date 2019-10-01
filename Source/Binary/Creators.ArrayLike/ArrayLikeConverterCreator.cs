using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly GenericConverterCreator creator = new GenericConverterCreator(new Dictionary<Type, Type>
        {
            [typeof(Memory<>)] = typeof(MemoryConverter<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryConverter<>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type) => creator.GetConverter(context, type);
    }
}
