using Mikodev.Binary.Internal.Components;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ListConverterCreator : GenericConverterCreator
    {
        public ListConverterCreator() : base(typeof(List<>), typeof(ListConverter<>)) { }
    }
}
