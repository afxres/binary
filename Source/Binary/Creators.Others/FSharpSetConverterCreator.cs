using Microsoft.FSharp.Collections;
using Mikodev.Binary.Internal.Components;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpSetConverterCreator : GenericConverterCreator
    {
        public FSharpSetConverterCreator() : base(typeof(FSharpSet<>), typeof(FSharpSetConverter<>)) { }
    }
}
