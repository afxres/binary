using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverterCreator : PatternConverterCreator
    {
        public ISetConverterCreator()
            : base(new[] { typeof(ISet<>) }, new Func<Type[], Type>[] { typeof(HashSet<>).MakeGenericType }, typeof(ISetConverter<,>))
        { }
    }
}
