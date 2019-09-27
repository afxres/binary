using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IListConverterCreator : PatternConverterCreator
    {
        public IListConverterCreator()
            : base(new[] { typeof(IEnumerable<>) }, new Func<Type[], Type>[] { x => x.Single().MakeArrayType(), typeof(List<>).MakeGenericType }, typeof(IListConverter<,>))
        { }
    }
}
