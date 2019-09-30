using Mikodev.Binary.Internal.Components;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class ISetConverterCreator : PatternConverterCreator
    {
        public ISetConverterCreator() : base(new[] { typeof(ISet<>) }, typeof(HashSet<>), typeof(ISetConverter<,>)) { }
    }
}
