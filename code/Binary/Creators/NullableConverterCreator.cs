using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            return CommonHelper.GetConverter(context, type, typeof(Nullable<>), typeof(NullableConverter<>), null);
        }
    }
}
