﻿using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            return CommonHelper.GetConverter(context, type, typeof(KeyValuePair<,>), typeof(KeyValuePairConverter<,>), x => x.CastArray<object>().Add(ContextMethods.GetItemLength(x)));
        }
    }
}
