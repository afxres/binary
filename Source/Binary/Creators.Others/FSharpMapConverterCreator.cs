using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal.Components;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpMapConverterCreator : GenericConverterCreator
    {
        public FSharpMapConverterCreator() : base(typeof(FSharpMap<,>), typeof(FSharpMapConverter<,>)) { }
    }
}
