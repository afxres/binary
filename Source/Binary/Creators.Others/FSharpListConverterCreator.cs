using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal.Components;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpListConverterCreator : GenericConverterCreator
    {
        public FSharpListConverterCreator() : base(typeof(FSharpList<>), typeof(FSharpListConverter<>)) { }
    }
}
