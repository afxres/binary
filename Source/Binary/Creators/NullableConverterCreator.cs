using Mikodev.Binary.Internal.Components;
using System;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : GenericConverterCreator
    {
        public NullableConverterCreator() : base(typeof(Nullable<>), typeof(NullableConverter<>)) { }
    }
}
