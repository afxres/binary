using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            return CommonHelper.GetConverter(context, type, typeof(KeyValuePair<,>), typeof(KeyValuePairConverter<,>), x => CommonHelper.Concat(x, (object)ContextMethods.GetItemLength(x)));
        }
    }
}
