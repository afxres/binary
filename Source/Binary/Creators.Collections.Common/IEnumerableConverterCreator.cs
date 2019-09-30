using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class IEnumerableConverterCreator : PatternConverterCreator
    {
        public IEnumerableConverterCreator() : base(new[] { typeof(IEnumerable<>) }, typeof(ArraySegment<>), typeof(IEnumerableConverter<,>)) { }
    }
}
