using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverterCreator : PatternConverterCreator
    {
        public IDictionaryConverterCreator()
            : base(new[] { typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>) }, new Func<Type[], Type>[] { typeof(Dictionary<,>).MakeGenericType }, typeof(IDictionaryConverter<,,>))
        { }
    }
}
