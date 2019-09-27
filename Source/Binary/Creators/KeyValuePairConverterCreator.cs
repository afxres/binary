using Mikodev.Binary.Internal.Components;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : GenericConverterCreator
    {
        public KeyValuePairConverterCreator() : base(typeof(KeyValuePair<,>), typeof(KeyValuePairConverter<,>)) { }
    }
}
