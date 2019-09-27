using Mikodev.Binary.Internal.Components;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverterCreator : PatternConverterCreator
    {
        public IDictionaryConverterCreator() : base(new[] { typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>) }, typeof(Dictionary<,>), typeof(IDictionaryConverter<,,>)) { }
    }
}
